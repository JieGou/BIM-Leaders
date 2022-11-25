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
    public class WallsParallelM : BaseModel
    {
        private double _toleranceAngle;
        private int _countWallsFiltered;

        private const string FILTER_NAME = "Check - Walls parallel.";

        #region PROPERTIES

        private double _distanceStepCm;
        public double DistanceStepCm
        {
            get { return _distanceStepCm; }
            set
            {
                _distanceStepCm = value;
                OnPropertyChanged(nameof(DistanceStepCm));
            }
        }

        private double _distanceToleranceCm;
        public double DistanceToleranceCm
        {
            get { return _distanceToleranceCm; }
            set
            {
                _distanceToleranceCm = value;
                OnPropertyChanged(nameof(DistanceToleranceCm));
            }
        }

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

        private int _selectedElement;
        public int SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                _selectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        #endregion

        public WallsParallelM(
            ExternalCommandData commandData,
            string transactionName,
            Action<string, RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction)
        {
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad.
        }

        #region METHODS

        private protected override void TryExecute()
        {
            ConvertUserInput();

            ICollection<Element> wallsToFilter = GetWallsToFilter();

            // Create annotations
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                if (wallsToFilter.Count != 0)
                {
                    Element filter = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME, wallsToFilter);
                    ViewFilterUtils.SetupFilter(_doc, filter, FilterColor);
                }

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        private void ConvertUserInput()
        {
            FilterColor = new Autodesk.Revit.DB.Color(
                FilterColorSystem.R,
                FilterColorSystem.G,
                FilterColorSystem.B);
        }

        /// <summary>
        /// Get walls from the current view that need to be set in filter.
        /// </summary>
        /// <returns>List of elements that can be added to filters later.</returns>
        private ICollection<Element> GetWallsToFilter()
        {
            ReferencePlane referencePlane = _doc.GetElement(new ElementId(SelectedElement)) as ReferencePlane;

            List<Element> wallsToFilter = new List<Element>();

            List<Wall> walls = GetWallsStraight();
            List<Wall> wallsPar = FilterWallsPar(walls, referencePlane);
            List<Wall> wallsPer = FilterWallsPer(walls, referencePlane);

            foreach (Wall wall in walls)
            {
                // Wall is not parallel or perpendicular.
                if (!wallsPar.Contains(wall) && !wallsPer.Contains(wall))
                    wallsToFilter.Add(wall);
            }

            _countWallsFiltered = wallsToFilter.Count;

            return wallsToFilter;
        }

        /// <summary>
        /// Get list of straight walls visible on active view.
        /// </summary>
        /// <returns>List of straight walls visible on active view.</returns>
        private List<Wall> GetWallsStraight()
        {
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                    .OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Wall>();

            // Filter line walls
            List<Wall> walls = new List<Wall>();
            foreach (Wall wall in wallsAll)
            {
                if (wall.Location is LocationCurve lc)
                    if (lc.Curve.GetType() == typeof(Line))
                        walls.Add(wall);
            }
            return walls;
        }

        /// <summary>
        /// Filter list of walls to get walls only parallel to the given reference plane with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls parallel to the reference plane.</returns>
        private List<Wall> FilterWallsPar(List<Wall> walls, ReferencePlane reference)
        {
            List<Wall> wallsPar = new List<Wall>();

            // Get direction of reference plane
            double referenceX = Math.Abs(reference.Direction.Y);
            double referenceY = Math.Abs(reference.Direction.X);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(referenceX) <= _toleranceAngle) && (Math.Abs(wallY) - Math.Abs(referenceY) <= _toleranceAngle))
                    wallsPar.Add(wall);
            }
            return wallsPar;
        }

        /// <summary>
        /// Filter list of walls to get walls only perpendicular to the given reference plane with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the reference plane.</returns>
        private List<Wall> FilterWallsPer(List<Wall> walls, ReferencePlane reference)
        {
            List<Wall> wallsPer = new List<Wall>();

            // Get direction of reference plane
            double referenceX = Math.Abs(reference.Direction.Y);
            double referenceY = Math.Abs(reference.Direction.X);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(referenceY) <= _toleranceAngle) && (Math.Abs(wallY) - Math.Abs(referenceX) <= _toleranceAngle))
                    wallsPer.Add(wall);
            }
            return wallsPer;
        }

        private protected override string GetRunResult()
        {
            string text = (_countWallsFiltered == 0)
                ? "All walls are clear"
                : $"{_countWallsFiltered} walls added to filter \"{FILTER_NAME}\".";

            return text;
        }

        #endregion
    }
}