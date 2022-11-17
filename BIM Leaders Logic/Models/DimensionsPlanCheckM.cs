using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class DimensionsPlanCheckM : BaseModel
    {
        private int _countWallsUndimensioned;
        private const string FILTER_NAME = "Check - Dimensions";

        #region PROPERTIES

        private System.Windows.Media.Color _filterColorSystem;
        public System.Windows.Media.Color FilterColorSystem
        {
            get { return _filterColorSystem; }
            set
            {
                _filterColorSystem = value;
                OnPropertyChanged(nameof(FilterColorSystem));
            }
        }

        private Autodesk.Revit.DB.Color _filterColor;
        public Autodesk.Revit.DB.Color FilterColor
        {
            get { return _filterColor; }
            set
            {
                _filterColor = value;
                OnPropertyChanged(nameof(FilterColor));
            }
        }

        #endregion

        public DimensionsPlanCheckM(ExternalCommandData commandData, string transactionName) : base(commandData, transactionName)
        {

        }

        #region IEXTERNALEVENTHANDLER

        public override void Execute(UIApplication app)
        {
            RunStarted = true;

            try
            {
                ConvertUserInput();

                List<ElementId> wallIds = GetWallIds();

                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    ElementId filter1Id = CreateFilter(wallIds);
                    _doc.Regenerate();
                    SetupFilter(filter1Id);

                    trans.Commit();
                }

                RunResult = GetRunResult();
            }
            catch (Exception e)
            {
                RunFailed = true;
                RunResult = ExceptionUtils.GetMessage(e);
            }
        }

        #endregion

        #region METHODS

        private void ConvertUserInput()
        {
            FilterColor = new Autodesk.Revit.DB.Color(
                FilterColorSystem.R,
                FilterColorSystem.G,
                FilterColorSystem.B);
        }

        /// <summary>
        /// Get walls that have no dimension references on active view.
        /// </summary>
        /// <returns>List<ElementId> of walls.</returns>
        private List<ElementId> GetWallIds()
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
        private ElementId CreateFilter(List<ElementId> elementIds)
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
        private void SetupFilter(ElementId filterId)
        {
            View view = _doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(FilterColor);
            overrideSettings.SetCutForegroundPatternId(patternId);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        private protected override string GetRunResult()
        {
            string text = "";

            text = (_countWallsUndimensioned == 0)
                ? "All walls are dimensioned"
                : $"{_countWallsUndimensioned} walls added to filter \"Check - Dimensions\".";

            return text;
        }

        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages) { return null; }

        #endregion
    }
}