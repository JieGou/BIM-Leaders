﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class WallsArranged : IExternalCommand
    {
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

            Options options = new Options
            {
                ComputeReferences = true
            };

            double toleranceAngle = uiapp.AngleTolerance / 100; // 0.001 grad
            //Color filterColor0 = new Color(255, 127, 39);
            //Color filterColor1 = new Color(255, 64, 64);
            string filterName0 = "Walls arranged filter. Distances";
            string filterName1 = "Walls arranged filter. Angles";

            try
            {
                WallsArrangedForm form = new WallsArrangedForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                WallsArrangedData data = form.DataContext as WallsArrangedData;
                double toleranceDistance = double.Parse(data.ResultDistanceTolerance);
                double distanceStep = double.Parse(data.ResultDistanceStep);
                Color filterColor0 = new Color(data.ResultColor0.R, data.ResultColor0.G, data.ResultColor0.B);
                Color filterColor1 = new Color(data.ResultColor1.R, data.ResultColor1.G, data.ResultColor1.B);

                // Getting References of Reference Planes
                IList<Reference> referenceUncheckedList = uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");
                // Checking for invalid selection
                if (referenceUncheckedList.Count != 2)
                {
                    TaskDialog.Show("Walls Arranged Check", "Wrong count of reference planes selected. Select 2 perendicular reference planes.");
                    return Result.Failed;
                }
                // Getting Reference planes
                ReferencePlane reference0 = doc.GetElement(referenceUncheckedList[0].ElementId) as ReferencePlane;
                ReferencePlane reference1 = doc.GetElement(referenceUncheckedList[1].ElementId) as ReferencePlane;
                // Checking for perpendicular input
                if ((Math.Abs(reference0.Direction.X) - Math.Abs(reference1.Direction.Y) <= toleranceAngle) && (Math.Abs(reference0.Direction.Y) - Math.Abs(reference1.Direction.X) <= toleranceAngle)) { }
                else
                {
                    TaskDialog.Show("Walls Arranged Check", "Selected reference planes are not perpendicular. Select 2 perendicular reference planes.");
                    return Result.Failed;
                }

                List<Wall> walls = GetWallsStraight(doc);
                List<Wall> wallsPar = FilterWallsPar(toleranceAngle, reference0, walls);
                List<Wall> wallsPer = FilterWallsPer(toleranceAngle, reference0, walls);

                // Get filters lists.
                List<Wall> wallsFilter = new List<Wall>();
                List<Wall> wallsFilterAngle = new List<Wall>();
                foreach (Wall wall in walls)
                {
                    // Checking if in parallel list.
                    if (wallsPar.Contains(wall))
                        wallsFilter.AddRange(FilterWallsDistance(toleranceDistance, distanceStep, reference0, wall));
                    // Checking if in perpendicular list.
                    else if (wallsPer.Contains(wall))
                        wallsFilter.AddRange(FilterWallsDistance(toleranceDistance, distanceStep, reference1, wall));
                    else
                        wallsFilterAngle.Add(wall);
                }

                using (Transaction trans = new Transaction(doc, "Create Filters for non-arranged Walls"))
                {
                    trans.Start();

                    if (wallsFilter.Count != 0)
                    {
                        ElementId filter0Id = CreateFilter(doc, filterName0, wallsFilter.ConvertAll(x => x.Id));
                        doc.Regenerate();
                        SetupFilter(doc, filter0Id, filterColor0);
                    }
                    if (wallsFilterAngle.Count != 0)
                    {
                        ElementId filter1Id = CreateFilter(doc, filterName1, wallsFilterAngle.ConvertAll(x => x.Id));
                        doc.Regenerate();
                        SetupFilter(doc, filter1Id, filterColor1);
                    }

                    trans.Commit();
                }

                // Show result
                if (wallsFilter.Count == 0 && wallsFilterAngle.Count == 0)
                    TaskDialog.Show("Walls arranged filter", "All walls are clear");
                else
                    TaskDialog.Show("Walls arranged filter", string.Format("{0} walls added to Walls arranged filter", (wallsFilter.Count + wallsFilterAngle.Count).ToString()));

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
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
                LocationCurve lc = wall.Location as LocationCurve;
                if (lc.Curve.GetType() == typeof(Line))
                    walls.Add(wall);
            }
            return walls;
        }

        /// <summary>
        /// Filter list of walls to get walls only parallel to the given reference plane with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls parallel to the reference plane.</returns>
        private static List<Wall> FilterWallsPar(double toleranceAngle, ReferencePlane reference, List<Wall> walls)
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
        private static List<Wall> FilterWallsPer(double toleranceAngle, ReferencePlane reference, List<Wall> walls)
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
        private static List<Wall> FilterWallsDistance(double toleranceDistance, double distanceStep, ReferencePlane reference, Wall wall)
        {
            List<Wall> wallsFilter = new List<Wall>();

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
#elif VERSION2021
            double distance = UnitUtils.ConvertFromInternalUnits(distanceInternal, UnitTypeId.Centimeters);
#endif
            // Calculate precision
            double precision = distance % distanceStep;
            if (0.5 - Math.Abs(0.5 - precision) > toleranceDistance)
                wallsFilter.Add(wall);

            return wallsFilter;
        }

        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// </summary>
        /// <returns>Created filter element Id.</returns>
        private static ElementId CreateFilter(Document doc, string filterName, List<ElementId> elementIds)
        {
            View view = doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
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
            IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
            ElementId pattern = patterns.First().Id;
            foreach (Element element in patterns)
                if (element.Name == "<Solid fill>")
                    pattern = element.Id;

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(pattern);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}