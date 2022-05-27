using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class WallsCompare : IExternalCommand
    {
        private class LinkSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                try
                {
                    if (element.Category.Name == "RVT Links")
                        return true;
                    return false;
                }
                catch (NullReferenceException) { return false; }
            }

            public bool AllowReference(Reference reference, XYZ point)
            {
                return false;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                // Show the dialog.
                WallsCompareForm form = new WallsCompareForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                WallsCompareData data = form.DataContext as WallsCompareData;
                
                bool inputLinks = data.ResultLinks;
                string materialName = doc.GetElement(data.ListMaterialsSelected).Name;
                FilledRegionType fill = doc.GetElement(data.ListFillTypesSelected) as FilledRegionType;
                
                double elevation = view.GenLevel.Elevation;
                
                // Links selection.

                ISelectionFilter selFilter = new LinkSelectionFilter();
                List<Wall> walls1 = new List<Wall>();
                List<Wall> walls2 = new List<Wall>();
                Transform transform1 = view.CropBox.Transform; // Just new transform, view is dummy
                Transform transform2 = view.CropBox.Transform; // Just new transform, view is dummy

                // If only one file is a link
                if (inputLinks)
                {
                    Reference linkReference = uidoc.Selection.PickObject(ObjectType.Element, selFilter, "Select Link");
                    RevitLinkInstance link1 = doc.GetElement(linkReference.ElementId) as RevitLinkInstance;

                    walls1 = GetWalls(link1.GetLinkDocument(), elevation, materialName);
                    walls2 = GetWalls(doc, elevation, materialName);

                    transform1 = link1.GetTotalTransform();
                    transform2 = transform1; // Don't need this because its doc file itself
                }
                else
                {
                    IList<Reference> linkReferences = uidoc.Selection.PickObjects(ObjectType.Element, selFilter, "Select 2 Links");
                    Reference linkReference1 = linkReferences.First();
                    Reference linkReference2 = linkReferences.Last();
                    RevitLinkInstance link1 = doc.GetElement(linkReference1.ElementId) as RevitLinkInstance;
                    RevitLinkInstance link2 = doc.GetElement(linkReference2.ElementId) as RevitLinkInstance;

                    walls1 = GetWalls(link1.GetLinkDocument(), elevation, materialName);
                    walls2 = GetWalls(link2.GetLinkDocument(), elevation, materialName);

                    transform1 = link1.GetTotalTransform();
                    transform2 = link2.GetTotalTransform();
                }

                // Getting plan loops of walls
                List<CurveLoop> loops1 = GetWallsLoops(walls1);
                List<CurveLoop> loops2 = GetWallsLoops(walls2);
                
                // Getting total plan loop of wall set
                Solid solid1 = GetWallsLoop(loops1);
                Solid solid2 = GetWallsLoop(loops2);

                // Transform solids
                Solid solid1Transformed = SolidUtils.CreateTransformed(solid1, transform1);
                Solid solid2Transformed = solid2;
                if (!inputLinks)
                    solid2Transformed = SolidUtils.CreateTransformed(solid2, transform2);

                if (solid1Transformed.Volume == 0 || solid2Transformed.Volume == 0)
                {
                    TaskDialog.Show("Walls comparison", "No intersections found.");
                    return Result.Failed;
                }

                // Get list of curveloops by intersecting two solids.
                List<CurveLoop> loopList = GetCurveLoops(solid1Transformed, solid2Transformed);

                // Drawing filled region
                using (Transaction trans = new Transaction(doc, "Compare Walls"))
                {
                    trans.Start();
                    
                    FilledRegion region = FilledRegion.Create(doc, fill.Id, doc.ActiveView.Id, loopList);

                    trans.Commit();
                }

                int count = loopList.Count;

                // Show result
                string text = $"{count} filled regions created.";
                TaskDialog.Show("Walls comparison", text);
                
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get walls from document, that have level with given elevation and have material with given name.
        /// </summary>
        /// <param name="doc">Document (current Revit file or link).</param>
        /// <param name="elevation">Elevation of the needed walls, input the current plan view level elevation.</param>
        /// <param name="material">If wall will have material with given name, it will be filtered in.</param>
        /// <returns>A list of walls from the document.</returns>
        private static List<Wall> GetWalls(Document doc, double elevation, string material)
        {
            List<Wall> walls = new List<Wall>();

            // Selecting all walls from doc
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>(); //LINQ function

            // Selecting all levels from doc
            IEnumerable<Level> levelsAll = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .WhereElementIsNotElementType()
                .Cast<Level>(); //LINQ function

            // Getting closest level in the document
            double level0 = levelsAll.First().Elevation;
            foreach (Level level in levelsAll)
                if (Math.Abs(level.ProjectElevation - elevation) < 1)
                    level0 = level.ProjectElevation;

            // Filtering needed walls for materials and height
            foreach (Wall wall in wallsAll)
            {
                CompoundStructure wallStructure = wall.WallType.GetCompoundStructure();
                if (wallStructure != null)
                {
                    IList<CompoundStructureLayer> layers = wallStructure.GetLayers();
                    List<string> materialNames = new List<string>();
                    foreach (CompoundStructureLayer layer in layers)
                        materialNames.Add(doc.GetElement(layer.MaterialId).Name);
                    if (materialNames.Contains(material)) // Filtering  for materials
                    {
                        LocationCurve wallLocation = wall.Location as LocationCurve;
                        double diff = Math.Abs(wallLocation.Curve.GetEndPoint(0).Z - level0);
                        if (diff < 1)
                            walls.Add(wall);
                    }
                }
            }
            return walls;
        }

        /// <summary>
        /// Convert walls contours to list of CurveLoop items.
        /// </summary>
        /// <param name="wall">Wall element.</param>
        /// <returns>List of CurveLoops.</returns>
        private static List<CurveLoop> GetWallsLoops(IEnumerable<Wall> walls)
        {
            List<CurveLoop> loops = new List<CurveLoop>();

            Options options = new Options();

            foreach (Wall wall in walls)
            {
                List<CurveLoop> loopsWall = new List<CurveLoop>();

                GeometryElement geometryElement = wall.get_Geometry(options);
                foreach (Solid geometrySolid in geometryElement)
                {
                    foreach (Face face in geometrySolid.Faces)
                    {
                        if (face is PlanarFace)
                        {
                            PlanarFace facePlanar = face as PlanarFace;
                            if (facePlanar.FaceNormal.Z == -1)
                            {
                                double height = facePlanar.Origin.Z;
                                double heightLowest = height;
                                // Get the lowest face
                                if (loopsWall.Count == 0)
                                    loopsWall = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                else
                                {
                                    if (height < heightLowest)
                                    {
                                        loopsWall = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                        heightLowest = height;
                                    }
                                }
                            }
                        }
                    }
                }
                if (loopsWall.Count > 0)
                    loops.AddRange(loopsWall);
            }
            return loops;
        }

        /// <summary>
        /// Convert walls loops to single solid.
        /// </summary>
        /// <returns>Solid</returns>
        private static Solid GetWallsLoop(List<CurveLoop> CurveLoopsInput)
        {
            List<Solid> solids = new List<Solid>();
            foreach (CurveLoop curveLoop in CurveLoopsInput)
            {
                List<CurveLoop> CurveLoops = new List<CurveLoop> { curveLoop };
                Solid extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(CurveLoops, new XYZ(0, 0, 1), 10);

                if (solids.Count > 0)
                {
                    Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation(extrusion, solids.First(), BooleanOperationsType.Union);
                    solids.Clear();
                    solids.Add(solid);
                }
                else
                    solids.Add(extrusion);
            }
            return solids.First();
        }

        /// <summary>
        /// Get list of curveloops by intersecting two solids.
        /// </summary>
        /// <returns>List of CurveLoop objects.</returns>
        private static List<CurveLoop> GetCurveLoops(Solid solid1, Solid solid2)
        {
            List<CurveLoop> loopList = new List<CurveLoop>();

            Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);

            // Create CurveLoop from intersection
            foreach (Face face in intersection.Faces)
            {
                try
                {
                    PlanarFace facePlanar = face as PlanarFace;
                    if (facePlanar.FaceNormal.Z == 1)
                    {
                        IList<CurveLoop> loops = face.GetEdgesAsCurveLoops();
                        loopList.AddRange(loops);
                    }
                }
                catch { }
            }
            return loopList;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
        }
    }
}