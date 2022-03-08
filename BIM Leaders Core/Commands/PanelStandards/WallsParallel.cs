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
    public class WallsParallel : IExternalCommand
    {
        public static Reference GetRefRef(UIDocument doc)
        {
            Reference lineReference = doc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Reference Plane");
            return lineReference;
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
                Color color = new Color(255, 127, 39);

                Options options = new Options
                {
                    ComputeReferences = true
                };

                ReferencePlane reference = doc.GetElement(GetRefRef(uidoc).ElementId) as ReferencePlane;

                // Get Walls
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> wallsAll = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Filter walls with no need to check
                List<Wall> walls = new List<Wall>();
                foreach(Wall wall in wallsAll)
                {
                    try
                    {
                        XYZ temp = wall.Orientation;
                        walls.Add(wall);
                    }
                    catch { }
                }

                // Get direction of reference plane
                double referenceX = Math.Abs(reference.Direction.X);
                double referenceY = Math.Abs(reference.Direction.Y);

                // Get lists of walls that are parallel and perpendicular
                List<Wall> wallsPar = new List<Wall>();
                List<Wall> wallsPer = new List<Wall>(); 
                foreach(Wall wall in walls)
                {
                    double wallX = Math.Abs(wall.Orientation.X);
                    double wallY = Math.Abs(wall.Orientation.Y);

                    // Checking if parallel
                    if(Math.Abs(wallX - referenceY) <= tolerance && Math.Abs(wallY - referenceX) <= tolerance)
                        wallsPar.Add(wall);
                    if (Math.Abs(wallX - referenceX) <= tolerance && Math.Abs(wallY - referenceY) <= tolerance)
                        wallsPer.Add(wall);
                }

                // Subtracting the lists from all walls list via set operation
                List<Wall> wallsFilter = new List<Wall>();
                foreach (Wall wall in walls)
                {
                    if (wallsPar.Contains(wall) | wallsPer.Contains(wall)) { }
                    else
                        wallsFilter.Add(wall);
                }

                // Converting to ICollection of Element Ids
                List<ElementId> wallsFilterIds = new List<ElementId>();
                foreach(Wall wall in wallsFilter)
                    wallsFilterIds.Add(wall.Id);

                using (Transaction trans = new Transaction(doc, "Create Filter for non-parallel Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach(Element element in filters)
                        if(element.Name == "Walls parallel filter")
                            doc.Delete(element.Id);

                    SelectionFilterElement filter = SelectionFilterElement.Create(doc, "Walls parallel filter");
                    filter.SetElementIds(wallsFilterIds);

                    // Add the filter to the view
                    ElementId filterId = filter.Id;
                    view.AddFilter(filterId);
                    doc.Regenerate();

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach(Element element in patterns)
                        if(element.Name == "<Solid fill>")
                            pattern = element.Id;

                    // Use the existing graphics settings, and change the color to Orange
                    OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
                    overrideSettings.SetCutForegroundPatternColor(color);
                    overrideSettings.SetCutForegroundPatternId(pattern);
                    view.SetFilterOverrides(filterId, overrideSettings);

                    trans.Commit();
                }

                // Show result
                if (wallsFilter.Count == 0)
                    TaskDialog.Show("Walls parallel filter", "All walls are clear");
                else
                    TaskDialog.Show("Walls parallel filter", string.Format("{0} walls added to Walls parallel filter", wallsFilter.Count.ToString()));

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
            return typeof(WallsParallel).Namespace + "." + nameof(WallsParallel);
        }
    }
}