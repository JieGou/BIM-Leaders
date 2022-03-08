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
    public class DimensionsPlanCheck : IExternalCommand
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
                IEnumerable<Wall> wallsAll = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Wall>();

                // Get Dimensions
                FilteredElementCollector collector1 = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Dimension> dimensionsAll = collector1.OfClass(typeof(Dimension))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Dimension>();

                // List for creating a filter
                List<ElementId> wallFilterIds = new List<ElementId>();

                // Iterate walls
                foreach (Wall wall in wallsAll)
                {
                    XYZ normalWall = new XYZ(0,0,0);
                    try
                    {
                        normalWall = wall.Orientation;
                    }
                    catch { continue; }
                    // List for intersections count for each dimansion
                    List<int> countIntersections = new List<int>();
                    // Iterate dimensions
                    foreach (Dimension dimension in dimensionsAll)
                    {
                        Line dimensionLine = dimension.Curve as Line;
                        if (dimensionLine != null)
                        {
                            dimensionLine.MakeBound(0, 1);
                            XYZ point0 = dimensionLine.GetEndPoint(0);
                            XYZ point1 = dimensionLine.GetEndPoint(1);
                            XYZ dimensionLoc = point1.Subtract(point0).Normalize();
                            // Intersections count
                            int countIntersection = 0;

                            ReferenceArray referenceArray = dimension.References;
                            // Iterate dimension references
                            foreach (Reference reference in referenceArray)
                                if (reference.ElementId == wall.Id)
                                    countIntersection++;
                            // If 2 dimensions on a wall so check if dimansion is parallel to wall normal
                            if (countIntersection >= 2)
                                if (Math.Round(Math.Abs((dimensionLoc.AngleTo(normalWall) / Math.PI - 0.5) * 2)) == 1) // Angle is from 0 to PI, so divide by PI - from 0 to 1, then...
                                    countIntersections.Add(countIntersection);
                        }
                    }
                    // Check if no dimensions left
                    if (countIntersections.Count == 0)
                        wallFilterIds.Add(wall.Id);
                }

                using (Transaction trans = new Transaction(doc, "Create Filter for non-dimensioned Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach (Element f in filters)
                        if (f.Name == "Walls dimension filter")
                            doc.Delete(f.Id);

                    SelectionFilterElement filter = SelectionFilterElement.Create(doc, "Walls dimension filter");
                    filter.SetElementIds(wallFilterIds);

                    // Add the filter to the view
                    ElementId filterId = filter.Id;
                    view.AddFilter(filterId);
                    doc.Regenerate();

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach (Element p in patterns)
                        if (p.Name == "<Solid fill>")
                            pattern = p.Id;

                    // Use the existing graphics settings, and change the color to Orange
                    OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
                    overrideSettings.SetCutForegroundPatternColor(color);
                    overrideSettings.SetCutForegroundPatternId(pattern);
                    view.SetFilterOverrides(filterId, overrideSettings);
                    
                    trans.Commit();
                }

                // Show result
                if (wallFilterIds.Count == 0)
                    TaskDialog.Show("Dimension Plan Check", "All walls are dimensioned");
                else
                    TaskDialog.Show("Dimension Plan Check", string.Format("{0} walls added to Walls dimension filter", wallFilterIds.Count.ToString()));

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
            return typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
        }
    }
}
