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
                    {
                        return true;
                    }
                    return false;
                }
                catch (System.NullReferenceException) { return false; }
            }

            public bool AllowReference(Reference reference, XYZ point)
            {
                return false;
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

            FilteredElementCollector collector0 = new FilteredElementCollector(doc);
            FilteredElementCollector collector1 = new FilteredElementCollector(doc);

            // Selecting all walls from doc 0
            IEnumerable<Wall> wallsAll = collector0.OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>(); //LINQ function

            // Selecting all levels from doc 0
            IEnumerable<Level> levelsAll = collector1.OfCategory(BuiltInCategory.OST_Levels)
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
            foreach(Solid geometrySolid in geometryElement)
            {
                foreach(Face face in geometrySolid.Faces)
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
        private static Solid GetWallsLoop(List<CurveLoop> loops)
        {
            List<Solid> temp_booleaned_list = new List<Solid>();
            foreach (CurveLoop l in loops)
            {
                List<CurveLoop> l_c = new List<CurveLoop> { l };
                Solid temp_extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(l_c, new XYZ(0, 0, 1), 10);

                if (temp_booleaned_list.Count > 0)
                {
                    Solid b = BooleanOperationsUtils.ExecuteBooleanOperation(temp_extrusion, temp_booleaned_list.First(), BooleanOperationsType.Union);
                    temp_booleaned_list.Clear();
                    temp_booleaned_list.Add(b);
                }
                else
                    temp_booleaned_list.Add(temp_extrusion);
            }
            return temp_booleaned_list.First();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get View Id
            View view = doc.ActiveView;

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

                int count = 0;
                
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
                Solid loop1T = SolidUtils.CreateTransformed(loop1, transform1);
                Solid loop2T = loop2;
                if (!inputLinks)
                    loop2T = SolidUtils.CreateTransformed(loop2, transform2);

                
                IList<CurveLoop> loop = new List<CurveLoop>();
                List<IList<CurveLoop>> loopList = new List<IList<CurveLoop>>();
                if (loop1T.Volume > 0 && loop2T.Volume > 0)
                {
                    Solid boolean = BooleanOperationsUtils.ExecuteBooleanOperation(loop1T, loop2T, BooleanOperationsType.Intersect);

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
                    using (Transaction trans = new Transaction(doc, "Compare Walls"))
                    {
                        trans.Start();

                        // Drawing filled region
                        foreach (IList<CurveLoop> l in loopList)
                        {
                            FilledRegion region = FilledRegion.Create(doc, fill.Id, view.Id, l);
                            count++;
                        }

                        trans.Commit();
                    }
                    TaskDialog.Show("Walls comparison", string.Format("{0} filled regions created.", count.ToString()));
                }
                else
                    TaskDialog.Show("Walls comparison", string.Format("No intersections found."));
                
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
        }
    }
}