using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
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

            string filterName = "Walls dimension filter";

            try
            {
                DimensionsPlanCheckForm form = new DimensionsPlanCheckForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DimensionsPlanCheckData data = form.DataContext as DimensionsPlanCheckData;
                Color filterColor = new Color(data.ResultColor.R, data.ResultColor.G, data.ResultColor.B);

                List<ElementId> wallIds = GetWallIds(doc);

                using (Transaction trans = new Transaction(doc, "Create Filter for non-dimensioned Walls"))
                {
                    trans.Start();

                    ElementId filter1Id = CreateFilter(doc, filterName, wallIds);
                    doc.Regenerate();
                    SetupFilter(doc, filter1Id, filterColor);

                    trans.Commit();
                }
                ShowResult(wallIds.Count);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get walls that have no dimension references on active view.
        /// </summary>
        /// <returns>List<ElementId> of walls.</returns>
        private static List<ElementId> GetWallIds(Document doc)
        {
            List<ElementId> wallIds = new List<ElementId>();

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

            // Iterate walls
            foreach (Wall wall in wallsAll)
            {
                XYZ normalWall = new XYZ(0, 0, 0);
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
                    wallIds.Add(wall.Id);
            }
            return wallIds;
        }

        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// </summary>
        /// <returns>Created filter element Id.</returns>
        private static ElementId CreateFilter(Document doc, string filterName, List<ElementId> elementIds)
        {
            View view = doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(doc)
                .OfClass(typeof(SelectionFilterElement))
                .ToElements();
            foreach (Element element in filters)
                if (element.Name == filterName)
                    doc.Delete(element.Id);

            SelectionFilterElement filter = SelectionFilterElement.Create(doc, filterName);
            filter.SetElementIds(elementIds);

            // Add the filter to the view
            ElementId filterId = filter.Id;
            view.AddFilter(filterId);

            return filterId;
        }

        /// <summary>
        /// Change filter settings. Must be applied after regeneration when filter is new.
        /// </summary>
        private static void SetupFilter(Document doc, ElementId filterId, Color filterColor)
        {
            View view = doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(patternId);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        private static void ShowResult(int count)
        {
            // Show result
            string text = (count == 0)
                ? "All walls are dimensioned"
                : $"{count} walls added to Walls dimension filter";

            TaskDialog.Show("Dimension Plan Check", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
        }
    }
}
