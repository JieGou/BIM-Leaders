using System;
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

            try
            {
                double toleranceAngle = uiapp.AngleTolerance / 100; // 0.001 grad
                Color color0 = new Color(255, 127, 39);
                Color color1 = new Color(255, 64, 64);

                Options options = new Options
                {
                    ComputeReferences = true
                };


                // Collector for data provided in window
                WallsArrangedData data = new WallsArrangedData();

                WallsArrangedForm form = new WallsArrangedForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as WallsArrangedData;
                double distanceStep = double.Parse(data.ResultDistanceStep);
                double toleranceDistance = double.Parse(data.ResultDistanceTolerance);


                // Getting References of Reference Planes
                IList<Reference> referenceUncheckedList = uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");
                // Checking for invalid selection
                if (referenceUncheckedList.Count != 2)
                {
                    TaskDialog.Show("Walls Arranged Check", string.Format("Wrong count of reference planes selected. Select 2 perendicular reference planes."));
                    return Result.Failed;
                }
                // Getting Reference planes
                ReferencePlane reference0 = doc.GetElement(referenceUncheckedList[0].ElementId) as ReferencePlane;
                ReferencePlane reference1 = doc.GetElement(referenceUncheckedList[1].ElementId) as ReferencePlane;
                // Checking for perpendicular input
                if ((Math.Abs(reference0.Direction.X) - Math.Abs(reference1.Direction.Y) <= toleranceAngle) && (Math.Abs(reference0.Direction.Y) - Math.Abs(reference1.Direction.X) <= toleranceAngle)) { }
                else
                {
                    TaskDialog.Show("Walls Arranged Check", string.Format("Selected reference planes are not perpendicular. Select 2 perendicular reference planes."));
                    return Result.Failed;
                }

                // Get Walls
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> wallsAll = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Filter walls with no need to check
                List<Wall> walls = new List<Wall>();
                foreach(Wall wall in wallsAll)
                {
                    try
                    {
                        XYZ temp = wall.Orientation;
                        walls.Add(wall);
                    }
                    catch { }
                }

                // Get direction of reference plane
                double reference0X = Math.Abs(reference0.Direction.Y);
                double reference0Y = Math.Abs(reference0.Direction.X);

                // Get lists of walls that are parallel and perpendicular
                List<Wall> wallsPar = new List<Wall>();
                List<Wall> wallsPer = new List<Wall>(); 
                foreach(Wall wall in walls)
                {
                    double wallOX = Math.Abs(wall.Orientation.X);
                    double wallOY = Math.Abs(wall.Orientation.Y);

                    // Checking if parallel
                    if ((Math.Abs(wallOX) - Math.Abs(reference0X) <= toleranceAngle) && (Math.Abs(wallOY) - Math.Abs(reference0Y) <= toleranceAngle))
                        wallsPar.Add(wall);
                    if ((Math.Abs(wallOX) - Math.Abs(reference0Y) <= toleranceAngle) && (Math.Abs(wallOY) - Math.Abs(reference0X) <= toleranceAngle))
                        wallsPer.Add(wall);
                }

                // Subtracting the lists from all walls list via set operation
                List<Wall> wallsFilter = new List<Wall>();
                List<Wall> wallsFilterAngle = new List<Wall>();
                foreach (Wall wall in walls)
                {
                    // Checking if in parallel list
                    if (wallsPar.Contains(wall))
                    {
                        // Checking the distance
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
                        XYZ point = new XYZ(reference0.BubbleEnd.X, reference0.BubbleEnd.Y, wallLocation.Curve.GetEndPoint(0).Z);

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
                    }
                    // Checking if in perpendicular list
                    else if (wallsPer.Contains(wall))
                    {
                        // Checking the distance
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
                        XYZ point = new XYZ(reference1.BubbleEnd.X, reference1.BubbleEnd.Y, wallLocation.Curve.GetEndPoint(0).Z);

                        // Check the orientation of exterior side of the wall, if its towards the point p
                        XYZ wallOrientation = wall.Orientation;
                        XYZ projected = wallLocationCurve.Project(point).XYZPoint;
                        XYZ difference = point - projected;

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
                    }
                    else
                        wallsFilterAngle.Add(wall);
                }

                // Converting to ICollection of Element Ids
                List<ElementId> wallFilterIds = new List<ElementId>();
                foreach (Wall wall in wallsFilter)
                    wallFilterIds.Add(wall.Id);
                List<ElementId> wallFilterAngleIds = new List<ElementId>();
                foreach (Wall wall in wallsFilterAngle)
                    wallFilterAngleIds.Add(wall.Id);

                using (Transaction trans = new Transaction(doc, "Create Filters for non-arranged Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach(Element element in filters)
                        if(element.Name == "Walls arranged filter. Distances" || element.Name == "Walls arranged filter. Angles")
                            doc.Delete(element.Id);

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach (Element element in patterns)
                        if (element.Name == "<Solid fill>")
                            pattern = element.Id;

                    // Add filters to the view
                    if (wallsFilter.Count > 0)
                    {
                        SelectionFilterElement filter0 = SelectionFilterElement.Create(doc, "Walls arranged filter. Distances");
                        filter0.SetElementIds(wallFilterIds);
                        ElementId filter0Id = filter0.Id;
                        view.AddFilter(filter0Id);

                        doc.Regenerate();

                        // Use the existing graphics settings, and change the color
                        OverrideGraphicSettings overrideSettings0 = view.GetFilterOverrides(filter0Id);
                        overrideSettings0.SetCutForegroundPatternColor(color0);
                        overrideSettings0.SetCutForegroundPatternId(pattern);
                        view.SetFilterOverrides(filter0Id, overrideSettings0);
                    }
                    if (wallsFilterAngle.Count > 0)
                    {
                        SelectionFilterElement filter1 = SelectionFilterElement.Create(doc, "Walls arranged filter. Angles");
                        filter1.SetElementIds(wallFilterAngleIds);
                        ElementId filter1Id = filter1.Id;
                        view.AddFilter(filter1Id);

                        doc.Regenerate();

                        OverrideGraphicSettings overrideSettings1 = view.GetFilterOverrides(filter1Id);
                        overrideSettings1.SetCutForegroundPatternColor(color1);
                        overrideSettings1.SetCutForegroundPatternId(pattern);
                        view.SetFilterOverrides(filter1Id, overrideSettings1);
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
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}