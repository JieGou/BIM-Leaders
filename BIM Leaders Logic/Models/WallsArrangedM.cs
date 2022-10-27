﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArrangedM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private double _toleranceAngle;
        private int _countWallsFilteredDistance;
        private int _countWallsFilteredAngle;

        private const string TRANSACTION_NAME = "Annotate Section";
        private const string FILTER_NAME_DISTANCE = "Check - Walls arranging. Distances";
        private const string FILTER_NAME_ANGLE = "Check - Walls arranging. Angles";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

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

        private double _distanceStep;
        public double DistanceStep
        {
            get { return _distanceStep; }
            set
            {
                _distanceStep = value;
                OnPropertyChanged(nameof(DistanceStep));
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

        private double _distanceTolerance;
        public double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set
            {
                _distanceTolerance = value;
                OnPropertyChanged(nameof(DistanceTolerance));
            }
        }

        private System.Windows.Media.Color _filterColorAngleSystem;
        public System.Windows.Media.Color FilterColorAngleSystem
        {
            get { return _filterColorAngleSystem; }
            set
            {
                _filterColorAngleSystem = value;
                OnPropertyChanged(nameof(FilterColorAngleSystem));
            }
        }

        private Autodesk.Revit.DB.Color _filterColorAngle;
        public Autodesk.Revit.DB.Color FilterColorAngle
        {
            get { return _filterColorAngle; }
            set
            {
                _filterColorAngle = value;
                OnPropertyChanged(nameof(FilterColorAngle));
            }
        }

        private System.Windows.Media.Color _filterColorDistanceSystem;
        public System.Windows.Media.Color FilterColorDistanceSystem
        {
            get { return _filterColorDistanceSystem; }
            set
            {
                _filterColorDistanceSystem = value;
                OnPropertyChanged(nameof(FilterColorDistanceSystem));
            }
        }

        private Autodesk.Revit.DB.Color _filterColorDistance;
        public Autodesk.Revit.DB.Color FilterColorDistance
        {
            get { return _filterColorDistance; }
            set
            {
                _filterColorDistance = value;
                OnPropertyChanged(nameof(FilterColorDistance));
            }
        }

        private List<int> _selectedElements;
        public List<int> SelectedElements
        {
            get { return _selectedElements; }
            set
            {
                _selectedElements = value;
                OnPropertyChanged(nameof(SelectedElements));
            }
        }

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            { 
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        #endregion

        public WallsArrangedM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad.
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TRANSACTION_NAME;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                ConvertUserInput();

                (ICollection<Element> wallsToFilterDistance, ICollection<Element> wallsToFilterAngle) = GetWallsToFilter();

                // Create annotations
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    if (wallsToFilterDistance.Count != 0)
                    {
                        Element filter0 = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME_DISTANCE, wallsToFilterDistance);
                        ViewFilterUtils.SetupFilter(_doc, filter0, FilterColorDistance);
                    }
                    if (wallsToFilterAngle.Count != 0)
                    {
                        Element filter1 = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME_ANGLE, wallsToFilterAngle);
                        ViewFilterUtils.SetupFilter(_doc, filter1, FilterColorAngle);
                    }

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }

            ShowResult();
        }

        #endregion

        #region METHODS

        private void ConvertUserInput()
        {
#if VERSION2020
            DistanceStep = UnitUtils.ConvertToInternalUnits(DistanceStepCm, DisplayUnitType.DUT_CENTIMETERS);
            DistanceTolerance = UnitUtils.ConvertToInternalUnits(DistanceToleranceCm, DisplayUnitType.DUT_CENTIMETERS);
#else
            DistanceStep = UnitUtils.ConvertToInternalUnits(DistanceStepCm, UnitTypeId.Centimeters);
            DistanceTolerance = UnitUtils.ConvertToInternalUnits(DistanceToleranceCm, UnitTypeId.Centimeters);
#endif

            FilterColorAngle = new Autodesk.Revit.DB.Color(
                FilterColorAngleSystem.R,
                FilterColorAngleSystem.G,
                FilterColorAngleSystem.B);

            FilterColorDistance = new Autodesk.Revit.DB.Color(
                FilterColorDistanceSystem.R,
                FilterColorDistanceSystem.G,
                FilterColorDistanceSystem.B);
        }

        /// <summary>
        /// Get walls from the current view that need to be set in filter.
        /// </summary>
        /// <returns>Tuple of 2 elements lists that can be added to filters later.</returns>
        private (ICollection<Element>, ICollection<Element>) GetWallsToFilter()
        {
            ReferencePlane referencePlane0 = _doc.GetElement(new ElementId(SelectedElements[0])) as ReferencePlane;
            ReferencePlane referencePlane1 = _doc.GetElement(new ElementId(SelectedElements[1])) as ReferencePlane;

            List<Element> wallsToFilterDistn = new List<Element>();
            List<Element> wallsToFilterAngle = new List<Element>();

            List<Wall> walls = GetWallsStraight();
            List<Wall> wallsPar = FilterWallsPar(walls, referencePlane0);
            List<Wall> wallsPer = FilterWallsPer(walls, referencePlane0);

            foreach (Wall wall in walls)
            {
                // Checking if in parallel list.
                if (wallsPar.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, referencePlane0));
                // Checking if in perpendicular list.
                else if (wallsPer.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, referencePlane1));
                else
                    wallsToFilterAngle.Add(wall);
            }

            _countWallsFilteredDistance = wallsToFilterDistn.Count;
            _countWallsFilteredAngle = wallsToFilterAngle.Count;

            return (wallsToFilterDistn, wallsToFilterAngle);
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

        /// <summary>
        /// Check if walls have good distance to a reference plane.
        /// </summary>
        /// <returns>List of walls that have bad distance to the given reference plane.</returns>
        private List<Wall> FilterWallsDistance(Wall wall, ReferencePlane reference)
        {
            List<Wall> wallsFilter = new List<Wall>();

            if (!(wall.Location is LocationCurve))
            {
                return wallsFilter;
            }

            LocationCurve wallLocation = wall.Location as LocationCurve;

            // Make unbound because Distance is calculating differently then just by normal, if point is out of curve range
            Curve wallLocationCurve = wallLocation.Curve;
            wallLocationCurve.MakeUnbound();

            // Calculate offset. Because Wall Location is always on the center, so offset it to an edge of structure layer
            double lineOffset = 0;
            if (wall.WallType.Kind == WallKind.Basic)
            {
                double wallWidth = wall.WallType.Width;
                int wallCoreIndex = wall.WallType.GetCompoundStructure().GetFirstCoreLayerIndex();

                // Get thickness of all exterior layers together
                double wallWidthExterior = 0;
                for (int i = 0; i < wallCoreIndex; i++)
                    wallWidthExterior += wall.WallType.GetCompoundStructure().GetLayerWidth(i);

                // Offset from Wall Center to the Wall Exterior Core line
                lineOffset = wallWidth / 2 - wallWidthExterior;
            }

            // Point
            XYZ point = new XYZ(reference.BubbleEnd.X, reference.BubbleEnd.Y, wallLocation.Curve.GetEndPoint(0).Z);

            // Check the orientation of exterior side of the wall, if its towards the point p
            XYZ wallOrientation = wall.Orientation;
            XYZ pointProjected = wallLocationCurve.Project(point).XYZPoint;
            XYZ difference = point - pointProjected;

            // Calculate the distance
            double distanceInternal = 0;
            if (difference.AngleTo(wallOrientation) < 1)
                distanceInternal = wallLocationCurve.Project(point).Distance - lineOffset;
            else
                distanceInternal = wallLocationCurve.Project(point).Distance + lineOffset;
#if VERSION2020
            double distance = UnitUtils.ConvertFromInternalUnits(distanceInternal, DisplayUnitType.DUT_CENTIMETERS);
#else
            double distance = UnitUtils.ConvertFromInternalUnits(distanceInternal, UnitTypeId.Centimeters);
#endif
            // Calculate precision
            double precision = distance % DistanceStep;
            if (0.5 - Math.Abs(0.5 - precision) > DistanceTolerance)
                wallsFilter.Add(wall);

            return wallsFilter;
        }

        private void ShowResult()
        {
            if (RunResult.Length == 0)
            {
                if (_countWallsFilteredDistance + _countWallsFilteredAngle == 0)
                    RunResult = "All walls are clear";
                else
                {
                    if (_countWallsFilteredDistance > 0)
                        RunResult += $"{_countWallsFilteredDistance} walls added to filter \"{FILTER_NAME_DISTANCE}\".";
                    if (_countWallsFilteredAngle > 0)
                    {
                        if (RunResult.Length > 0)
                            RunResult += " ";
                        RunResult += $"{_countWallsFilteredAngle} walls added to filter \"{FILTER_NAME_ANGLE}\".";
                    }
                }
            }

            TaskDialog.Show(TRANSACTION_NAME, RunResult);
        }

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
