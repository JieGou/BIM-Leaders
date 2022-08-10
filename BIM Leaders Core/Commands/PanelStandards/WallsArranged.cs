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
    [TransactionAttribute(TransactionMode.Manual)]
    public class WallsArranged : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document.
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            double toleranceAngle = doc.Application.AngleTolerance / 100; // 0.001 grad.

            string filterName0 = "Walls arranged filter. Distances";
            string filterName1 = "Walls arranged filter. Angles";

            try
            {
                WallsArrangedForm form = new WallsArrangedForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window.
                WallsArrangedData data = form.DataContext as WallsArrangedData;
                double toleranceDistance = data.ResultDistanceTolerance;
                double distanceStep = data.ResultDistanceStep;
                Color filterColor0 = new Color(data.ResultColor0.R, data.ResultColor0.G, data.ResultColor0.B);
                Color filterColor1 = new Color(data.ResultColor1.R, data.ResultColor1.G, data.ResultColor1.B);

                // Getting References of Reference Planes.
                IList<Reference> referenceUncheckedList = uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");
                
                // Checking for invalid selection.
                if (referenceUncheckedList.Count != 2)
                {
                    TaskDialog.Show("Walls Arranged Check", "Wrong count of reference planes selected. Select 2 perendicular reference planes.");
                    return Result.Failed;
                }

                // Getting Reference planes.
                ReferencePlane reference0 = doc.GetElement(referenceUncheckedList[0].ElementId) as ReferencePlane;
                ReferencePlane reference1 = doc.GetElement(referenceUncheckedList[1].ElementId) as ReferencePlane;
                
                // Checking for perpendicular input
                if (reference0.Direction.DotProduct(reference1.Direction) > toleranceAngle)
                {
                    TaskDialog.Show("Walls Arranged Check", "Selected reference planes are not perpendicular. Select 2 perendicular reference planes.");
                    return Result.Failed;
                }

                (ICollection<Element> wallsToFilterDistn, ICollection<Element> wallsToFilterAngle) = GetWallsToFilter(doc, reference0, reference1, toleranceAngle, toleranceDistance, distanceStep);

                using (Transaction trans = new Transaction(doc, "Create Filters for non-arranged Walls"))
                {
                    trans.Start();

                    if (wallsToFilterDistn.Count != 0)
                    {
                        Element filter0 = ViewFilterUtils.CreateSelectionFilter(doc, filterName0, wallsToFilterDistn);
                        ViewFilterUtils.SetupFilter(doc, filter0, filterColor0);
                    }
                    if (wallsToFilterAngle.Count != 0)
                    {
                        Element filter1 = ViewFilterUtils.CreateSelectionFilter(doc, filterName1, wallsToFilterAngle);
                        ViewFilterUtils.SetupFilter(doc, filter1, filterColor1);
                    }

                    trans.Commit();
                }
                ShowResult(wallsToFilterDistn.Count + wallsToFilterAngle.Count);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get walls from the current view that need to be set in filter.
        /// </summary>
        /// <returns>Tuple of 2 elements lists that can be added to filters later.</returns>
        private static (ICollection<Element>, ICollection<Element>) GetWallsToFilter(Document doc, ReferencePlane reference0, ReferencePlane reference1, double toleranceAngle, double toleranceDistance, double distanceStep)
        {
            List<Element> wallsToFilterDistn = new List<Element>();
            List<Element> wallsToFilterAngle = new List<Element>();

            List<Wall> walls = GetWallsStraight(doc);
            List<Wall> wallsPar = FilterWallsPar(walls, reference0, toleranceAngle);
            List<Wall> wallsPer = FilterWallsPer(walls, reference0, toleranceAngle);

            foreach (Wall wall in walls)
            {
                // Checking if in parallel list.
                if (wallsPar.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, reference0, toleranceDistance, distanceStep));
                // Checking if in perpendicular list.
                else if (wallsPer.Contains(wall))
                    wallsToFilterDistn.AddRange(FilterWallsDistance(wall, reference1, toleranceDistance, distanceStep));
                else
                    wallsToFilterAngle.Add(wall);
            }

            return (wallsToFilterDistn, wallsToFilterAngle);
        }

        /// <summary>
        /// Get list of straight walls visible on active view.
        /// </summary>
        /// <returns>List of straight walls visible on active view.</returns>
        private static List<Wall> GetWallsStraight(Document doc)
        {
            IEnumerable<Wall> wallsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
        private static List<Wall> FilterWallsPar(List<Wall> walls, ReferencePlane reference, double toleranceAngle)
        {
            List<Wall> wallsPar = new List<Wall>();

            // Get direction of reference plane
            double referenceX = Math.Abs(reference.Direction.Y);
            double referenceY = Math.Abs(reference.Direction.X);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(referenceX) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(referenceY) <= toleranceAngle))
                    wallsPar.Add(wall);
            }
            return wallsPar;
        }

        /// <summary>
        /// Filter list of walls to get walls only perpendicular to the given reference plane with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the reference plane.</returns>
        private static List<Wall> FilterWallsPer(List<Wall> walls, ReferencePlane reference, double toleranceAngle)
        {
            List<Wall> wallsPer = new List<Wall>();

            // Get direction of reference plane
            double referenceX = Math.Abs(reference.Direction.Y);
            double referenceY = Math.Abs(reference.Direction.X);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(referenceY) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(referenceX) <= toleranceAngle))
                    wallsPer.Add(wall);
            }
            return wallsPer;
        }

        /// <summary>
        /// Check if walls have good distance to a reference plane.
        /// </summary>
        /// <returns>List of walls that have bad distance to the given reference plane.</returns>
        private static List<Wall> FilterWallsDistance(Wall wall, ReferencePlane reference, double toleranceDistance, double distanceStep)
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
            double precision = distance % distanceStep;
            if (0.5 - Math.Abs(0.5 - precision) > toleranceDistance)
                wallsFilter.Add(wall);

            return wallsFilter;
        }

        private static void ShowResult(int count)
        {
            // Show result
            string text = (count == 0)
                ? "All walls are clear"
                : $"{count} walls added to Walls arranged filter";

            TaskDialog.Show("Walls arranged filter", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path.
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}