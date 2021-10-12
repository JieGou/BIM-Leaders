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
    public class Walls_Compare : IExternalCommand
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

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
        /// <summary>
        /// Get walls from document, that has level with given elevation and have material with given name
        /// </summary>
        /// <param name="doc">Document (current Revit file or link).</param>
        /// <param name="elevation">Elevation of the needed walls, calculating as current plan view level elevation.</param>
        /// <param name="mat_name">If wall will have material with given name, it will be filtered in.</param>
        /// <returns></returns>
        private static List<Wall> GetWalls(Document doc, double elevation, string mat_name)
        {
            List<Wall> walls = new List<Wall>();

            FilteredElementCollector collector_0 = new FilteredElementCollector(doc);
            FilteredElementCollector collector_1 = new FilteredElementCollector(doc);

            // Selecting all walls from doc 0
            IEnumerable<Wall> walls_all = collector_0.OfCategory(BuiltInCategory.OST_Walls)
                .WhereElementIsNotElementType()
                .Cast<Wall>(); //LINQ function

            // Selecting all levels from doc 0
            IEnumerable<Level> levels_all = collector_1.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>(); //LINQ function

            // Getting closest level in the document
            double level_0 = levels_all.First().Elevation;
            foreach (Level i in levels_all)
                if (Math.Abs(i.ProjectElevation - elevation) < 1)
                    level_0 = i.ProjectElevation;

            // Filtering needed walls for materials and height
            foreach (Wall w in walls_all)
            {
                CompoundStructure cs = w.WallType.GetCompoundStructure();
                if (cs != null)
                {
                    IList<CompoundStructureLayer> mats = cs.GetLayers();
                    List<ElementId> mats_id = new List<ElementId>();
                    List<string> mats_name = new List<string>();
                    foreach (CompoundStructureLayer m in mats)
                        mats_name.Add(doc.GetElement(m.MaterialId).Name);
                    if (mats_name.Contains(mat_name)) // Filtering  for materials
                    {
                        LocationCurve wall_lc = w.Location as LocationCurve;
                        double diff = Math.Abs(wall_lc.Curve.GetEndPoint(0).Z - level_0);
                        if (diff < 1)
                            walls.Add(w);
                    }
                }
            }
            return walls;
        }
        private static Reference GetLinkRef(UIDocument doc)
        {
            Reference link_ref = doc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("RVT Links"), "Select Link");
            return link_ref;
        }
        private static List<CurveLoop> GetWallsLoops(Wall wall)
        {
            Options opt = new Options();

            List<CurveLoop> loops = new List<CurveLoop>();
            GeometryElement geom_elem = wall.get_Geometry(opt);
            foreach(Solid geom_solid in geom_elem)
            {
                foreach(Face face in geom_solid.Faces)
                {
                    if (face is PlanarFace)
                    {
                        PlanarFace face_pl = face as PlanarFace;
                        if (face_pl.FaceNormal.Z == -1)
                        {
                            double h = face_pl.Origin.Z;
                            double h_lowest = h;
                            // Get the lowest face
                            if (loops.Count == 0)
                            {
                                loops = face_pl.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                // h_lowest = h;
                            }
                            else
                            {
                                if (h < h_lowest)
                                {
                                    loops = face_pl.GetEdgesAsCurveLoops() as List<CurveLoop>;
                                    h_lowest = h;
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
                Walls_Compare_Data data = new Walls_Compare_Data(uidoc);

                // Create a form to select objects.
                bool? result = null;
                while (result == null)
                {
                    if (result == false)
                        return Result.Cancelled;
                    // Show the dialog.
                    Walls_Compare_Form form = new Walls_Compare_Form(uidoc);
                    result = form.ShowDialog();
                    // Get user provided information from window
                    data = form.DataContext as Walls_Compare_Data;
                }
                
                bool result_links = data.result_links;
                string mat_name = doc.GetElement(data.mats_list_sel).Name;
                FilledRegionType fill = doc.GetElement(data.fill_types_list_sel) as FilledRegionType;
                
                double elevation = view.GenLevel.Elevation;

                int count = 0;
                
                // Links selection

                ISelectionFilter isf = new LinkSelectionFilter();
                List<Wall> walls_file_1 = new List<Wall>();
                List<Wall> walls_file_2 = new List<Wall>();
                Transform file_1_transform = view.CropBox.Transform; // Just new transform, view is dummy
                Transform file_2_transform = view.CropBox.Transform; // Just new transform, view is dummy

                if (result_links)
                {
                    Reference link_ref = uidoc.Selection.PickObject(ObjectType.Element, isf, "Select Link");
                    RevitLinkInstance link_1 = doc.GetElement(link_ref.ElementId) as RevitLinkInstance;

                    walls_file_1 = GetWalls(link_1.GetLinkDocument(), elevation, mat_name);
                    walls_file_2 = GetWalls(doc, elevation, mat_name);

                    file_1_transform = link_1.GetTotalTransform();
                    file_2_transform = file_1_transform; // Don't need this because its doc file itself
                }
                else
                {
                    IList<Reference> links_ref = uidoc.Selection.PickObjects(ObjectType.Element, isf, "Select 2 Links");
                    Reference link_ref_1 = links_ref.First();
                    Reference link_ref_2 = links_ref.Last();
                    RevitLinkInstance link_1 = doc.GetElement(link_ref_1.ElementId) as RevitLinkInstance;
                    RevitLinkInstance link_2 = doc.GetElement(link_ref_2.ElementId) as RevitLinkInstance;

                    walls_file_1 = GetWalls(link_1.GetLinkDocument(), elevation, mat_name);
                    walls_file_2 = GetWalls(link_2.GetLinkDocument(), elevation, mat_name);

                    file_1_transform = link_1.GetTotalTransform();
                    file_2_transform = link_2.GetTotalTransform();
                }

                // Getting plan loops of walls
                List<CurveLoop> loops_file_1 = new List<CurveLoop>();
                foreach (Wall w in walls_file_1)
                    if (GetWallsLoops(w).Count > 0)
                        loops_file_1.AddRange(GetWallsLoops(w));

                List<CurveLoop> loops_file_2 = new List<CurveLoop>();
                foreach (Wall w in walls_file_2)
                    if (GetWallsLoops(w).Count > 0)
                        loops_file_2.AddRange(GetWallsLoops(w));
                
                // Getting plan loops of wall sets
                Solid loop_file_1 = GetWallsLoop(loops_file_1);
                Solid loop_file_2 = GetWallsLoop(loops_file_2);

                // Transform solids
                Solid loop_file_1_t = SolidUtils.CreateTransformed(loop_file_1, file_1_transform);
                Solid loop_file_2_t = loop_file_2;
                if (!result_links)
                    loop_file_2_t = SolidUtils.CreateTransformed(loop_file_2, file_2_transform);

                
                IList<CurveLoop> loop = new List<CurveLoop>();
                List<IList<CurveLoop>> loop_list = new List<IList<CurveLoop>>();
                if (loop_file_1_t.Volume > 0 && loop_file_2_t.Volume > 0)
                {
                    Solid boolean = BooleanOperationsUtils.ExecuteBooleanOperation(loop_file_1_t, loop_file_2_t, BooleanOperationsType.Intersect);

                    // Create CurveLoop from intersection
                    foreach (Face face in boolean.Faces)
                    {
                        try
                        {
                            PlanarFace face_planar = face as PlanarFace;
                            if (face_planar.FaceNormal.Z == 1)
                            {
                                loop = face.GetEdgesAsCurveLoops();
                                loop_list.Add(loop);
                            }
                        }
                        catch { }
                    }
                    using (Transaction trans = new Transaction(doc, "Compare Walls"))
                    {
                        trans.Start();

                        // Drawing filled region
                        foreach (IList<CurveLoop> l in loop_list)
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
            return typeof(Walls_Compare).Namespace + "." + nameof(Walls_Compare);
        }
    }
}