using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Walls_Arranged : IExternalCommand
    {
        public static IList<Reference> GetRefRef(UIDocument doc)
        {
            IList<Reference> lines_ref = doc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");
            return lines_ref;
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get Application
            Application uiapp = doc.Application;

            // Get View Id
            View view = doc.ActiveView;

            try
            {
                double tolerance = uiapp.AngleTolerance / 100; // 0.001 grad
                double distance_step = 1; // 1 cm
                double tolerance_distance = 0.00001; // 0.00001 cm
                Color color_0 = new Color(255, 127, 39);
                Color color_1 = new Color(255, 64, 64);

                Options options = new Options
                {
                    ComputeReferences = true
                };

                // Getting References of Reference Planes
                IList<Reference> reference_unchecked = GetRefRef(uidoc);
                // Checking for invalid selection
                if (reference_unchecked.Count != 2)
                {
                    TaskDialog.Show("Walls Arranged Check", string.Format("Wrong count of reference planes selected. Select 2 perendicular reference planes."));
                    return Result.Failed;
                }
                // Getting Reference planes
                ReferencePlane reference_0 = doc.GetElement(reference_unchecked[0].ElementId) as ReferencePlane;
                ReferencePlane reference_1 = doc.GetElement(reference_unchecked[1].ElementId) as ReferencePlane;
                // Checking for perpendicular input
                if ((Math.Abs(reference_0.Direction.X) - Math.Abs(reference_1.Direction.Y) <= tolerance) && (Math.Abs(reference_0.Direction.Y) - Math.Abs(reference_1.Direction.X) <= tolerance))
                { }
                else
                {
                    TaskDialog.Show("Walls Arranged Check", string.Format("Selected reference planes are not perpendicular. Select 2 perendicular reference planes."));
                    return Result.Failed;
                }

                // Get Walls
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> walls_all = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Filter walls with no need to check
                List<Wall> walls = new List<Wall>();
                foreach(Wall w in walls_all)
                {
                    try
                    {
                        XYZ temp = w.Orientation;
                        walls.Add(w);
                    }
                    catch { }
                }

                // Get direction of reference plane
                double ref_ox = Math.Abs(reference_0.Direction.Y);
                double ref_oy = Math.Abs(reference_0.Direction.X);

                // Get lists of walls that are parallel and perpendicular
                List<Wall> walls_par = new List<Wall>();
                List<Wall> walls_per = new List<Wall>(); 
                foreach(Wall w in walls)
                {
                    double ox = Math.Abs(w.Orientation.X);
                    double oy = Math.Abs(w.Orientation.Y);

                    // Checking if parallel
                    if ((Math.Abs(ox) - Math.Abs(ref_ox) <= tolerance) && (Math.Abs(oy) - Math.Abs(ref_oy) <= tolerance))
                    {
                        walls_par.Add(w);
                    }
                    if ((Math.Abs(ox) - Math.Abs(ref_oy) <= tolerance) && (Math.Abs(oy) - Math.Abs(ref_ox) <= tolerance))
                    {
                        walls_per.Add(w);
                    }
                }

                // Subtracting the lists from all walls list via set operation
                List<Wall> walls_filter = new List<Wall>();
                List<Wall> walls_filter_angle = new List<Wall>();
                foreach (Wall w in walls)
                {
                    // Checking if in parallel list
                    if (walls_par.Contains(w))
                    {
                        // Checking the distance
                        LocationCurve lc = w.Location as LocationCurve;
                        // Make unbound because Distance is calculating differently then just by normal, if point is out of curve range
                        Curve c = lc.Curve;
                        c.MakeUnbound();

                        // Calculate offset. Because Wall Location is always on the center, so offset it to an edge of structure layer
                        double l_offset = 0;
                        if (w.WallType.Kind == WallKind.Basic)
                        {
                            double l_all_int = w.WallType.Width;
                            int l_core_index = w.WallType.GetCompoundStructure().GetFirstCoreLayerIndex();

                            // Get thickness of all exterior layers together
                            double l_ext_int = 0;
                            for (int i = 0; i < l_core_index; i++)
                            {
                                l_ext_int += w.WallType.GetCompoundStructure().GetLayerWidth(i);
                            }

                            // Offset from Wall Center to the Wall Exterior Core line
                            l_offset = l_all_int / 2 - l_ext_int;
                        }

                        // Point
                        XYZ p = new XYZ(reference_0.BubbleEnd.X, reference_0.BubbleEnd.Y, lc.Curve.GetEndPoint(0).Z);

                        // Check the orientation of exterior side of the wall, if its towards the point p
                        XYZ o = w.Orientation;
                        XYZ projected = c.Project(p).XYZPoint;
                        XYZ difference = p - projected;

                        // Calculate the distance
                        double d_int = 0;
                        if (difference.AngleTo(o) < 1)
                            d_int = c.Project(p).Distance - l_offset;
                        else
                            d_int = c.Project(p).Distance + l_offset;
                        double d = UnitUtils.ConvertFromInternalUnits(d_int, DisplayUnitType.DUT_CENTIMETERS);

                        // Calculate precision
                        double precision = d % distance_step;
                        if (0.5 - Math.Abs(0.5 - precision) > tolerance_distance)
                            walls_filter.Add(w);
                    }
                    // Checking if in perpendicular list
                    else if (walls_per.Contains(w))
                    {
                        // Checking the distance
                        LocationCurve lc = w.Location as LocationCurve;
                        // Make unbound because Distance is calculating differently then just by normal, if point is out of curve range
                        Curve c = lc.Curve;
                        c.MakeUnbound();

                        // Calculate offset. Because Wall Location is always on the center, so offset it to an edge of structure layer
                        double l_offset = 0;
                        if (w.WallType.Kind == WallKind.Basic)
                        {
                            double l_all_int = w.WallType.Width;
                            int l_core_index = w.WallType.GetCompoundStructure().GetFirstCoreLayerIndex();

                            // Get thickness of all exterior layers together
                            double l_ext_int = 0;
                            for (int i = 0; i < l_core_index; i++)
                            {
                                l_ext_int += w.WallType.GetCompoundStructure().GetLayerWidth(i);
                            }

                            // Offset from Wall Center to the Wall Exterior Core line
                            l_offset = l_all_int / 2 - l_ext_int;
                        }

                        // Point
                        XYZ p = new XYZ(reference_1.BubbleEnd.X, reference_1.BubbleEnd.Y, lc.Curve.GetEndPoint(0).Z);

                        // Check the orientation of exterior side of the wall, if its towards the point p
                        XYZ o = w.Orientation;
                        XYZ projected = c.Project(p).XYZPoint;
                        XYZ difference = p - projected;

                        // Calculate the distance
                        double d_int = 0;
                        if (difference.AngleTo(o) < 1)
                            d_int = c.Project(p).Distance - l_offset;
                        else
                            d_int = c.Project(p).Distance + l_offset;
                        double d = UnitUtils.ConvertFromInternalUnits(d_int, DisplayUnitType.DUT_CENTIMETERS);

                        // Calculate precision
                        double precision = d % distance_step;
                        if (0.5 - Math.Abs(0.5 - precision) > tolerance_distance)
                            walls_filter.Add(w);
                    }
                    else
                        walls_filter_angle.Add(w);
                }

                // Converting to ICollection of Element Ids
                List<ElementId> walls_filter_ids = new List<ElementId>();
                foreach (Wall w in walls_filter)
                {
                    walls_filter_ids.Add(w.Id);
                }
                List<ElementId> walls_filter_angle_ids = new List<ElementId>();
                foreach (Wall w in walls_filter_angle)
                {
                    walls_filter_angle_ids.Add(w.Id);
                }

                using (Transaction trans = new Transaction(doc, "Create Filters for non-arranged Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach(Element e in filters)
                    {
                        if(e.Name == "Walls arranged filter. Distances" || e.Name == "Walls arranged filter. Angles")
                            doc.Delete(e.Id);
                    }

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach (Element e in patterns)
                    {
                        if (e.Name == "<Solid fill>")
                            pattern = e.Id;
                    }

                    // Add filters to the view

                    if (walls_filter.Count > 0)
                    {
                        SelectionFilterElement filter_0 = SelectionFilterElement.Create(doc, "Walls arranged filter. Distances");
                        filter_0.SetElementIds(walls_filter_ids);
                        ElementId filterId_0 = filter_0.Id;
                        view.AddFilter(filterId_0);

                        doc.Regenerate();

                        // Use the existing graphics settings, and change the color
                        OverrideGraphicSettings overrideSettings_0 = view.GetFilterOverrides(filterId_0);
                        overrideSettings_0.SetCutForegroundPatternColor(color_0);
                        overrideSettings_0.SetCutForegroundPatternId(pattern);
                        view.SetFilterOverrides(filterId_0, overrideSettings_0);
                    }
                    if (walls_filter_angle.Count > 0)
                    {
                        SelectionFilterElement filter_1 = SelectionFilterElement.Create(doc, "Walls arranged filter. Angles");
                        filter_1.SetElementIds(walls_filter_angle_ids);
                        ElementId filterId_1 = filter_1.Id;
                        view.AddFilter(filterId_1);

                        doc.Regenerate();

                        OverrideGraphicSettings overrideSettings_1 = view.GetFilterOverrides(filterId_1);
                        overrideSettings_1.SetCutForegroundPatternColor(color_1);
                        overrideSettings_1.SetCutForegroundPatternId(pattern);
                        view.SetFilterOverrides(filterId_1, overrideSettings_1);
                    }
                    trans.Commit();
                }

                // Show result
                if (walls_filter.Count == 0 && walls_filter_angle.Count == 0)
                {
                    TaskDialog.Show("Walls arranged filter", "All walls are clear");
                }
                else
                {
                    TaskDialog.Show("Walls arranged filter", string.Format("{0} walls added to Walls arranged filter", (walls_filter.Count + walls_filter_angle.Count).ToString()));
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
            return typeof(Walls_Arranged).Namespace + "." + nameof(Walls_Arranged);
        }
    }
}