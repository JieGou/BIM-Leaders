using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionsPlanCheck : IExternalCommand
    {
        private static Document _doc;
        private static DimensionsPlanCheckData _inputData;
        private static int _countWallsUndimensioned;

        private const string TRANSACTION_NAME = "Create Filter for non-dimensioned Walls";
        private const string FILTER_NAME = "Check - Dimensions";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {

                List<ElementId> wallIds = GetWallIds();

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    ElementId filter1Id = CreateFilter(wallIds);
                    _doc.Regenerate();
                    SetupFilter(filter1Id);

                    trans.Commit();
                }
                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static DimensionsPlanCheckData GetUserInput()
        {
            DimensionsPlanCheckForm form = new DimensionsPlanCheckForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as DimensionsPlanCheckData;
        }

        /// <summary>
        /// Get walls that have no dimension references on active view.
        /// </summary>
        /// <returns>List<ElementId> of walls.</returns>
        private static List<ElementId> GetWallIds()
        {
            List<ElementId> wallIds = new List<ElementId>();

            // Get Walls.
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Wall>();

            // Get Dimensions.
            IEnumerable<Dimension> dimensionsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Dimension))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Dimension>();

            // Iterate walls.
            foreach (Wall wall in wallsAll)
            {
                XYZ normalWall = new XYZ(0, 0, 0);
                try
                {
                    normalWall = wall.Orientation;
                }
                catch { continue; }
                // List for intersections count for each dimansion.
                List<int> countIntersections = new List<int>();
                // Iterate dimensions.
                foreach (Dimension dimension in dimensionsAll)
                {
                    Line dimensionLine = dimension.Curve as Line;
                    if (dimensionLine != null)
                    {
                        dimensionLine.MakeBound(0, 1);
                        XYZ point0 = dimensionLine.GetEndPoint(0);
                        XYZ point1 = dimensionLine.GetEndPoint(1);
                        XYZ dimensionLoc = point1.Subtract(point0).Normalize();
                        // Intersections count.
                        int countIntersection = 0;

                        ReferenceArray referenceArray = dimension.References;
                        // Iterate dimension references.
                        foreach (Reference reference in referenceArray)
                            if (reference.ElementId == wall.Id)
                                countIntersection++;
                        // If 2 dimensions on a wall so check if dimansion is parallel to wall normal.
                        if (countIntersection >= 2)
                            if (Math.Round(Math.Abs((dimensionLoc.AngleTo(normalWall) / Math.PI - 0.5) * 2)) == 1) // Angle is from 0 to PI, so divide by PI - from 0 to 1, then...
                                countIntersections.Add(countIntersection);
                    }
                }
                // Check if no dimensions left.
                if (countIntersections.Count == 0)
                    wallIds.Add(wall.Id);
            }
            _countWallsUndimensioned = wallIds.Count;

            return wallIds;
        }

        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// </summary>
        /// <returns>Created filter element Id.</returns>
        private static ElementId CreateFilter(List<ElementId> elementIds)
        {
            View view = _doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(_doc)
                .OfClass(typeof(SelectionFilterElement))
                .ToElements();
            foreach (Element element in filters)
                if (element.Name == FILTER_NAME)
                    _doc.Delete(element.Id);

            SelectionFilterElement filter = SelectionFilterElement.Create(_doc, FILTER_NAME);
            filter.SetElementIds(elementIds);

            // Add the filter to the view
            ElementId filterId = filter.Id;
            view.AddFilter(filterId);

            return filterId;
        }

        /// <summary>
        /// Change filter settings. Must be applied after regeneration when filter is new.
        /// </summary>
        private static void SetupFilter(ElementId filterId)
        {
            View view = _doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

            Color filterColor = new Color(_inputData.ResultColor.R, _inputData.ResultColor.G, _inputData.ResultColor.B);

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(patternId);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countWallsUndimensioned == 0)
                ? "All walls are dimensioned"
                : $"{_countWallsUndimensioned} walls added to filter \"Check - Dimensions\".";

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
        }
    }
}
