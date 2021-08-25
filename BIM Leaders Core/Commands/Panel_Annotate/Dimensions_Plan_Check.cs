using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Dimensions_Plan_Check : IExternalCommand
    {
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
                Options options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = false,
                    View = view
                };

                Color color = new Color(255, 127, 39);

                double scale = view.Scale;
                XYZ zero = new XYZ(0,0,0);

                // Get Walls
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Wall> walls_all = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Wall>();

                // Get Dimensions
                FilteredElementCollector collector_d = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Dimension> dimensions_all = collector_d.OfClass(typeof(Dimension))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Dimension>();

                // List for creating a filter
                List<ElementId> wall_filter_ids = new List<ElementId>();

                // Iterate walls
                foreach (Wall w in walls_all)
                {
                    XYZ normal_wall = new XYZ(0,0,0);
                    try
                    {
                        normal_wall = w.Orientation;
                    }
                    catch { continue; }
                    // List for intersectins count for each dimansion
                    List<int> count_ints = new List<int>();
                    // Iterate dimensions
                    foreach (Dimension d in dimensions_all)
                    {
                        Line dim_line = d.Curve as Line;
                        if (dim_line != null)
                        {
                            dim_line.MakeBound(0, 1);
                            XYZ pt1 = dim_line.GetEndPoint(0);
                            XYZ pt2 = dim_line.GetEndPoint(1);
                            XYZ dim_loc = pt2.Subtract(pt1).Normalize();
                            // Intersections count
                            int count_int = 0;

                            ReferenceArray ref_array = d.References;
                            // Iterate dimension references
                            foreach (Reference r in ref_array)
                            {
                                if (r.ElementId == w.Id)
                                    count_int++;
                            }
                            // If 2 dimensions on a wall so check if dimansion is parallel to wall normal
                            if (count_int >= 2)
                            {
                                if (Math.Round(Math.Abs((dim_loc.AngleTo(normal_wall) / Math.PI - 0.5) * 2)) == 1) // Angle is from 0 to PI, so divide by PI - from 0 to 1, then...
                                    count_ints.Add(count_int);
                            }
                        }
                    }
                    // Check if no dimensions left
                    if (count_ints.Count == 0)
                        wall_filter_ids.Add(w.Id);
                }

                using (Transaction trans = new Transaction(doc, "Create Filter for non-dimensioned Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach (Element e in filters)
                    {
                        if (e.Name == "Walls dimension filter")
                        {
                            doc.Delete(e.Id);
                        }
                    }

                    SelectionFilterElement filter = SelectionFilterElement.Create(doc, "Walls dimension filter");
                    filter.SetElementIds(wall_filter_ids);

                    // Add the filter to the view
                    ElementId filterId = filter.Id;
                    view.AddFilter(filterId);
                    doc.Regenerate();

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach (Element e in patterns)
                    {
                        if (e.Name == "<Solid fill>")
                        {
                            pattern = e.Id;
                        }
                    }

                    // Use the existing graphics settings, and change the color to Orange
                    OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
                    overrideSettings.SetCutForegroundPatternColor(color);
                    overrideSettings.SetCutForegroundPatternId(pattern);
                    view.SetFilterOverrides(filterId, overrideSettings);
                    
                    trans.Commit();
                }

                // Show result
                if (wall_filter_ids.Count == 0)
                {
                    TaskDialog.Show("Dimension Plan Check", "All walls are dimensioned");
                }
                else
                {
                    TaskDialog.Show("Dimension Plan Check", string.Format("{0} walls added to Walls dimension filter", wall_filter_ids.Count.ToString()));
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
            return typeof(Dimensions_Plan_Check).Namespace + "." + nameof(Dimensions_Plan_Check);
        }
    }
}
