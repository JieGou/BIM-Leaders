using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Walls_Parallel : IExternalCommand
    {
        public class LineSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Reference Planes")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
        public static Reference GetRefRef(UIDocument doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new LineSelectionFilter();
            Reference line_ref = doc.Selection.PickObject(ObjectType.Element, selFilter, "Select Reference Plane");
            return line_ref;
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

            // Get View Id
            ElementId view_id = doc.ActiveView.Id;

            try
            {
                double tolerance = uiapp.AngleTolerance / 100; // 0.001 grad
                Color color = new Color(255, 127, 39);

                Options options = new Options();
                options.ComputeReferences = true;

                ReferencePlane reference = doc.GetElement(GetRefRef(uidoc).ElementId) as ReferencePlane;

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
                    catch
                    {

                    }
                }

                // Get direction of reference plane
                double ref_ox = Math.Abs(reference.Direction.Y);
                double ref_oy = Math.Abs(reference.Direction.X);

                // Get lists of walls that are parallel and perpendicular
                List<Wall> walls_par = new List<Wall>();
                List<Wall> walls_per = new List<Wall>(); 
                foreach(Wall w in walls)
                {
                    double ox = Math.Abs(w.Orientation.X);
                    double oy = Math.Abs(w.Orientation.Y);

                    // Checking if parallel
                    if(Math.Abs(ox-ref_ox) <= tolerance && Math.Abs(oy-ref_oy) <= tolerance)
                    {
                        walls_par.Add(w);
                    }
                    if (Math.Abs(ox - ref_oy) <= tolerance && Math.Abs(oy - ref_ox) <= tolerance)
                    {
                        walls_per.Add(w);
                    }
                }

                // Subtracting the lists from all walls list via set operation
                List<Wall> walls_filter = new List<Wall>();
                foreach (Wall w in walls)
                {
                    if (walls_par.Contains(w) | walls_per.Contains(w))
                    {
                        
                    }
                    else
                    {
                        walls_filter.Add(w);
                    }
                }

                // Converting to ICollection of Element Ids
                List<ElementId> walls_filter_ids = new List<ElementId>();
                foreach(Wall w in walls_filter)
                {
                    walls_filter_ids.Add(w.Id);
                }

                using (Transaction trans = new Transaction(doc, "Create Filter for non-parallel Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach(Element e in filters)
                    {
                        if(e.Name == "Walls parallel filter")
                        {
                            doc.Delete(e.Id);
                        }
                    }

                    SelectionFilterElement filter = SelectionFilterElement.Create(doc, "Walls parallel filter");
                    filter.SetElementIds(walls_filter_ids);

                    // Add the filter to the view
                    ElementId filterId = filter.Id;
                    view.AddFilter(filterId);
                    doc.Regenerate();

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach(Element e in patterns)
                    {
                        if(e.Name == "<Solid fill>")
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
                if (walls_filter.Count == 0)
                {
                    TaskDialog.Show("Walls parallel filter", "All walls are clear");
                }
                else
                {
                    TaskDialog.Show("Walls parallel filter", string.Format("{0} walls added to Walls parallel filter", walls_filter.Count.ToString()));
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}