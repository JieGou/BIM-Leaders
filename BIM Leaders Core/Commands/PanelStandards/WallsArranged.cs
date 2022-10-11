using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArranged : IExternalCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static double _toleranceAngle;
        private static int _countWallsFilteredDistance;
        private static int _countWallsFilteredAngle;
        private static WallsArrangedVM _inputData;

        private const string TRANSACTION_NAME = "Walls Arranged Check";
        private const string FILTER_NAME_DISTANCE = "Check - Walls arranging. Distances";
        private const string FILTER_NAME_ANGLE = "Check - Walls arranging. Angles";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad.

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                (ReferencePlane referencePlane0, ReferencePlane referencePlane1) = GetReferencePlanes();
                if (referencePlane0 == null)
                    return Result.Failed;

                (ICollection<Element> wallsToFilterDistance, ICollection<Element> wallsToFilterAngle) = GetWallsToFilter(referencePlane0, referencePlane1);

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    if (wallsToFilterDistance.Count != 0)
                    {
                        Element filter0 = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME_DISTANCE, wallsToFilterDistance);
                        ViewFilterUtils.SetupFilter(_doc, filter0, new Color(_inputData.ResultColor0.R, _inputData.ResultColor0.G, _inputData.ResultColor0.B));
                    }
                    if (wallsToFilterAngle.Count != 0)
                    {
                        Element filter1 = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME_ANGLE, wallsToFilterAngle);
                        ViewFilterUtils.SetupFilter(_doc, filter1, new Color(_inputData.ResultColor1.R, _inputData.ResultColor1.G, _inputData.ResultColor1.B));
                    }

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

        private static WallsArrangedVM GetUserInput()
        {
            WallsArrangedForm form = new WallsArrangedForm();
            WallsArrangedVM formVM = new WallsArrangedVM();
            form.DataContext = formVM;
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window.
            return form.DataContext as WallsArrangedVM;
        }

        private static (ReferencePlane, ReferencePlane) GetReferencePlanes()
        {
            // Getting References of Reference Planes.
            IList<Reference> referenceUncheckedList = _uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");

            // Checking for invalid selection.
            if (referenceUncheckedList.Count != 2)
            {
                TaskDialog.Show(TRANSACTION_NAME, "Wrong count of reference planes selected. Select 2 perendicular reference planes.");
                return (null, null);
            }

            // Getting Reference planes.
            ReferencePlane reference0 = _doc.GetElement(referenceUncheckedList[0].ElementId) as ReferencePlane;
            ReferencePlane reference1 = _doc.GetElement(referenceUncheckedList[1].ElementId) as ReferencePlane;

            // Checking for perpendicular input
            if (reference0.Direction.DotProduct(reference1.Direction) > _toleranceAngle)
            {
                TaskDialog.Show(TRANSACTION_NAME, "Selected reference planes are not perpendicular. Select 2 perendicular reference planes.");
                return (null, null);
            }

            return (reference0, reference1);
        }

        /// <summary>
        /// Get walls from the current view that need to be set in filter.
        /// </summary>
        /// <returns>Tuple of 2 elements lists that can be added to filters later.</returns>
        private static (ICollection<Element>, ICollection<Element>) GetWallsToFilter(ReferencePlane reference0, ReferencePlane reference1)
        {
            List<Element> wallsToFilterDistn = new List<Element>();
            List<Element> wallsToFilterAngle = new List<Element>();

            List<Wall> walls = GetWallsStraight();
            List<Wall> wallsPar = FilterWallsPar(walls, reference0);
            List<Wall> wallsPer = FilterWallsPer(walls, reference0);

            foreach (Wall wall in walls)
            {
                // Checking if in parallel list.
                if (wallsPar.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, reference0));
                // Checking if in perpendicular list.
                else if (wallsPer.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, reference1));
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
        private static List<Wall> GetWallsStraight()
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
        private static List<Wall> FilterWallsPar(List<Wall> walls, ReferencePlane reference)
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
        private static List<Wall> FilterWallsPer(List<Wall> walls, ReferencePlane reference)
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
        private static List<Wall> FilterWallsDistance(Wall wall, ReferencePlane reference)
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
            double precision = distance % _inputData.ResultDistanceStep;
            if (0.5 - Math.Abs(0.5 - precision) > _inputData.ResultDistanceTolerance)
                wallsFilter.Add(wall);

            return wallsFilter;
        }

        private static void ShowResult()
        {
            // Show result
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

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}