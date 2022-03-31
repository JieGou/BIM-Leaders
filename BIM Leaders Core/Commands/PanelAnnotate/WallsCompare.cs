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

            // Get View Id
            View view = doc.ActiveView;

            int count = 0;

            try
            {
                WallsCompareData data = new WallsCompareData(uidoc);

                // Create a form to select objects.
                bool? result = null;
                while (result == null)
                {
                    if (result == false)
                        return Result.Cancelled;
                    // Show the dialog.
                    WallsCompareForm form = new WallsCompareForm(uidoc);
                    result = form.ShowDialog();
                    // Get user provided information from window
                    data = form.DataContext as WallsCompareData;
                }
                
                bool inputLinks = data.ResultLinks;
                string materialName = doc.GetElement(data.ListMaterialsSelected).Name;
                FilledRegionType fill = doc.GetElement(data.ListFillTypesSelected) as FilledRegionType;
                
                double elevation = view.GenLevel.Elevation;
                
                // Links selection

                ISelectionFilter selFilter = new LinkSelectionFilter();
                List<Wall> walls1 = new List<Wall>();
                List<Wall> walls2 = new List<Wall>();
                Transform transform1 = view.CropBox.Transform; // Just new transform, view is dummy
                Transform transform2 = view.CropBox.Transform; // Just new transform, view is dummy

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
                List<CurveLoop> loops1 = new List<CurveLoop>();
                foreach (Wall wall in walls1)
                    if (GetWallsLoops(wall).Count > 0)
                        loops1.AddRange(GetWallsLoops(wall));

                List<CurveLoop> loops2 = new List<CurveLoop>();
                foreach (Wall wall in walls2)
                    if (GetWallsLoops(wall).Count > 0)
                        loops2.AddRange(GetWallsLoops(wall));
                
                // Getting plan loops of wall sets
                Solid loop1 = GetWallsLoop(loops1);
                Solid loop2 = GetWallsLoop(loops2);

                // Transform solids
                Solid loop1Transformed = SolidUtils.CreateTransformed(loop1, transform1);
                Solid loop2Transformed = loop2;
                if (!inputLinks)
                    loop2Transformed = SolidUtils.CreateTransformed(loop2, transform2);

                
                IList<CurveLoop> loop = new List<CurveLoop>();
                List<IList<CurveLoop>> loopList = new List<IList<CurveLoop>>();

                if (loop1Transformed.Volume == 0 || loop2Transformed.Volume == 0)
                {
                    TaskDialog.Show("Walls comparison", "No intersections found.");
                    return Result.Failed;
                }

                Solid boolean = BooleanOperationsUtils.ExecuteBooleanOperation(loop1Transformed, loop2Transformed, BooleanOperationsType.Intersect);

                // Create CurveLoop from intersection
                foreach (Face face in boolean.Faces)
                {
                    try
                    {
                        PlanarFace facePlanar = face as PlanarFace;
                        if (facePlanar.FaceNormal.Z == 1)
                        {
                            loop = face.GetEdgesAsCurveLoops();
                            loopList.Add(loop);
                        }
                    }
                    catch { }
                }

                // Drawing filled region
                using (Transaction trans = new Transaction(doc, "Compare Walls"))
                {
                    trans.Start();
                    
                    foreach (IList<CurveLoop> l in loopList)
                    {
                        FilledRegion region = FilledRegion.Create(doc, fill.Id, doc.ActiveView.Id, l);
                        count++;
                    }

                    trans.Commit();
                }

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
        private static List<CurveLoop> GetWallsLoops(Wall wall)
        {
            Options options = new Options();

            List<CurveLoop> loops = new List<CurveLoop>();
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
                            if (loops.Count == 0)
                                loops = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                            else
                            {
                                if (height < heightLowest)
                                {
                                    loops = facePlanar.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                    heightLowest = height;
                                }
                            }
                        }
                    }
                }
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

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
        }
    }
}