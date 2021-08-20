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
        public static Reference GetLinkRef(UIDocument doc)
        {
            Reference link_ref = doc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("RVT Links"), "Select Link");
            return link_ref;
        }
        public static List<CurveLoop> GetWallsLoops(Wall wall)
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
        public static Solid GetWallsLoop(List<CurveLoop> loops)
        {
            List<Solid> temp_booleaned_list = new List<Solid>();
            foreach (CurveLoop l in loops)
            {
                try
                {
                    List<CurveLoop> l_c = new List<CurveLoop> { l };
                    Solid temp_extrusion = GeometryCreationUtilities.CreateExtrusionGeometry(l_c, new XYZ(0, 0, 1), 10);

                    if (temp_booleaned_list.Count > 0)
                    {
                        Solid b = BooleanOperationsUtils.ExecuteBooleanOperation(temp_extrusion, temp_booleaned_list[0], BooleanOperationsType.Union);
                        temp_booleaned_list.Clear();
                        temp_booleaned_list.Add(b);
                    }
                    else
                    {
                        temp_booleaned_list.Add(temp_extrusion);
                    }
                }
                catch { }
            }
            return temp_booleaned_list[0];
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
                // Collector for data provided in window
                Walls_Compare_Data data = new Walls_Compare_Data(uidoc);

                Walls_Compare_Form form = new Walls_Compare_Form(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Walls_Compare_Data;

                string mat_name = doc.GetElement(data.fill_types_list_sel).Name;
                FilledRegionType fill = doc.GetElement(data.mats_list_sel) as FilledRegionType;
                string fill_name = fill.Name;
                double elevation = view.GenLevel.Elevation;
                int count = 0;

                RevitLinkInstance link = doc.GetElement(GetLinkRef(uidoc).ElementId) as RevitLinkInstance;

                Options options = new Options();

                // Selecting all walls from link on the view
                FilteredElementCollector collector_link_w = new FilteredElementCollector(link.GetLinkDocument());
                IEnumerable<Wall> walls_link_all = collector_link_w.OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .Cast<Wall>(); //LINQ function

                // Selecting all levels from the link
                FilteredElementCollector collector_link_l = new FilteredElementCollector(link.GetLinkDocument());
                IEnumerable<Level> levels_link_all = collector_link_l.OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType()
                    .Cast<Level>(); //LINQ function
                
                // Getting closest level in the link
                double level_0 = levels_link_all.First().Elevation;
                foreach(Level i in levels_link_all)
                {
                    if(Math.Abs(i.ProjectElevation - elevation) < 1)
                    {
                        level_0 = i.ProjectElevation;
                    }
                }
                
                // Selecting all walls from doc on the view
                FilteredElementCollector collector_walls = new FilteredElementCollector(doc);
                IEnumerable<Wall> walls_doc_all = collector_walls.OfCategory(BuiltInCategory.OST_Walls)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Wall>(); //LINQ function
                
                // Filtering needed walls for materials and height (for link)
                List<Wall> walls_link = new List<Wall>();
                foreach (Wall w in walls_link_all)
                {
                    CompoundStructure cs = w.WallType.GetCompoundStructure();
                    if (cs != null)
                    {
                        IList<CompoundStructureLayer> mats = cs.GetLayers();
                        List<ElementId> mats_id = new List<ElementId>();
                        List<string> mats_name = new List<string>();
                        foreach (CompoundStructureLayer c in mats)
                        {
                            mats_id.Add(c.MaterialId);
                            mats_name.Add(link.GetLinkDocument().GetElement(c.MaterialId).Name);
                            if (mats_name.Contains(mat_name))
                            {
                                LocationCurve wall_lc = w.Location as LocationCurve;
                                double diff = Math.Abs(wall_lc.Curve.GetEndPoint(0).Z - level_0);
                                if (diff < 1)
                                {
                                    walls_link.Add(w);
                                }
                            }
                        }
                    }
                }
                // Filtering needed walls for materials and height (for doc)
                List<Wall> walls_doc = new List<Wall>();
                foreach (Wall w in walls_doc_all)
                {
                    CompoundStructure cs = w.WallType.GetCompoundStructure();
                    if (cs != null)
                    {
                        IList<CompoundStructureLayer> mats = cs.GetLayers();
                        List<ElementId> mats_id = new List<ElementId>();
                        List<string> mats_name = new List<string>();
                        foreach (CompoundStructureLayer c in mats)
                        {
                            mats_id.Add(c.MaterialId);
                            mats_name.Add(doc.GetElement(c.MaterialId).Name);
                            if (mats_name.Contains(mat_name))
                            {
                                LocationCurve wall_lc = w.Location as LocationCurve;
                                double diff = Math.Abs(wall_lc.Curve.GetEndPoint(0).Z - level_0);
                                if (diff < 1)
                                {
                                    walls_doc.Add(w);
                                }
                            }
                        }
                    }
                }
                
                // Getting plan loops of walls
                List<CurveLoop> loops_link = new List<CurveLoop>();
                foreach (Wall w in walls_link)
                {
                    if (GetWallsLoops(w).Count > 0)
                    {
                        loops_link.AddRange(GetWallsLoops(w));
                    }
                }
                List<CurveLoop> loops_doc = new List<CurveLoop>();
                foreach (Wall w in walls_doc)
                {
                    if (GetWallsLoops(w).Count > 0)
                    {
                        loops_doc.AddRange(GetWallsLoops(w));
                    }
                }

                // Getting plan loops of wall sets
                Solid loop_link = GetWallsLoop(loops_link);
                Solid loop_doc = GetWallsLoop(loops_doc);

                IList<CurveLoop> loop = new List<CurveLoop>();
                List<IList<CurveLoop>> loop_list = new List<IList<CurveLoop>>();
                if (loop_link.Volume > 0 && loop_doc.Volume > 0)
                {
                    Solid boolean = BooleanOperationsUtils.ExecuteBooleanOperation(loop_link, loop_doc, BooleanOperationsType.Intersect);

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
                {
                    TaskDialog.Show("Walls comparison", string.Format("No intersections found."));
                }
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