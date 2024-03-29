﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArrangedModel : BaseModel
    {
        private double _toleranceAngle;
        private int _countWallsFilteredDistance;
        private int _countWallsFilteredAngle;

        private const string FILTER_NAME_DISTANCE = "Check - Walls arranging. Distances";
        private const string FILTER_NAME_ANGLE = "Check - Walls arranging. Angles";

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

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            _toleranceAngle = Doc.Application.AngleTolerance / 100; // 0.001 grad.

            ConvertUserInput();

            (ICollection<Element> wallsToFilterDistance, ICollection<Element> wallsToFilterAngle) = GetWallsToFilter();

            // Create annotations
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                if (wallsToFilterDistance.Count != 0)
                {
                    Element filter0 = ViewFilterUtils.CreateSelectionFilter(Doc, FILTER_NAME_DISTANCE, wallsToFilterDistance);
                    ViewFilterUtils.SetupFilter(Doc, filter0, FilterColorDistance);
                }
                if (wallsToFilterAngle.Count != 0)
                {
                    Element filter1 = ViewFilterUtils.CreateSelectionFilter(Doc, FILTER_NAME_ANGLE, wallsToFilterAngle);
                    ViewFilterUtils.SetupFilter(Doc, filter1, FilterColorAngle);
                }

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        private void ConvertUserInput()
        {
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
            ReferencePlane referencePlane0 = Doc.GetElement(new ElementId(SelectedElements[0])) as ReferencePlane;
            ReferencePlane referencePlane1 = Doc.GetElement(new ElementId(SelectedElements[1])) as ReferencePlane;

            List<Element> wallsToFilterDistn = new List<Element>();
            List<Element> wallsToFilterAngle = new List<Element>();

            List<Wall> walls = GetWallsStraight();
            List<Wall> wallsPar = FilterWallsPar(walls, referencePlane0);
            List<Wall> wallsPer = FilterWallsPer(walls, referencePlane0);

            foreach (Wall wall in walls)
            {
                // Wall is not parallel or perpendicular.
                if (!wallsPar.Contains(wall) && !wallsPer.Contains(wall))
                {
                    wallsToFilterAngle.Add(wall);
                    continue;
                }

                // Checking distance to the according reference plane.
                bool distanceIsGood = (wallsPar.Contains(wall))
                    ? CheckWallDistance(wall, referencePlane0)
                    : CheckWallDistance(wall, referencePlane1);

                // Wall is not nice distanced.
                if (!distanceIsGood)
                    wallsToFilterDistn.Add(wall);
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
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
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
        /// <returns>True if distance is good, otherwise false.</returns>
        private bool CheckWallDistance(Wall wall, ReferencePlane referencePlane)
        {
            if (!(wall.Location is LocationCurve))
                return true;

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

            // Point on reference plane end.
            XYZ point = new XYZ(referencePlane.BubbleEnd.X, referencePlane.BubbleEnd.Y, wallLocation.Curve.GetEndPoint(0).Z);

            // Check the orientation of exterior side of the wall, if its towards the point p
            XYZ wallOrientation = wall.Orientation;
            XYZ pointProjected = wallLocationCurve.Project(point).XYZPoint;
            XYZ difference = point - pointProjected;

            // Calculate the distance
            double distance = (difference.AngleTo(wallOrientation) < 1)
                ? wallLocationCurve.Project(point).Distance - lineOffset
                : wallLocationCurve.Project(point).Distance + lineOffset;
#if VERSION2020
            double distanceCm = UnitUtils.ConvertFromInternalUnits(distance, DisplayUnitType.DUT_CENTIMETERS);
#else
            double distanceCm = UnitUtils.ConvertFromInternalUnits(distance, UnitTypeId.Centimeters);
#endif

            // Calculate precision
            double precision = distanceCm % DistanceStepCm;
            if (0.5 - Math.Abs(0.5 - precision) > DistanceToleranceCm)
                return false;

            return true;
        }

        private protected override string GetRunResult()
        {
            string text = "";

            if (_countWallsFilteredDistance + _countWallsFilteredAngle == 0)
                text = "All walls are clear";
            else
            {
                if (_countWallsFilteredDistance > 0)
                    text += $"{_countWallsFilteredDistance} walls added to filter \"{FILTER_NAME_DISTANCE}\".";
                if (_countWallsFilteredAngle > 0)
                {
                    if (text.Length > 0)
                        text += " ";
                    text += $"{_countWallsFilteredAngle} walls added to filter \"{FILTER_NAME_ANGLE}\".";
                }
            }

            return text;
        }

        #endregion
    }
}