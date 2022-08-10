using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class WallsParallel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            double toleranceAngle = doc.Application.AngleTolerance / 100; // 0.001 grad
            string filterName = "Walls parallel filter";
            Color filterColor = new Color(255, 127, 39);

            try
            {
                ReferencePlane reference = doc.GetElement(SelectReferencePlane(uidoc).ElementId) as ReferencePlane;

                List<Wall> walls = GetWallsStraight(doc);
                ICollection<Element> wallsFilter = FilterWalls(walls, reference, toleranceAngle) as ICollection<Element>;

                if (wallsFilter.Count == 0)
                    return Result.Succeeded;

                using (Transaction trans = new Transaction(doc, "Create Filter for non-parallel Walls"))
                {
                    trans.Start();

                    Element filter = ViewFilterUtils.CreateSelectionFilter(doc, filterName, wallsFilter);
                    ViewFilterUtils.SetupFilter(doc, filter, filterColor);

                    trans.Commit();
                }
                ShowResult(wallsFilter.Count);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Allow user to select an element of Reference Plane category.
        /// </summary>
        /// <returns>Reference as a result of user selection.</returns>
        private static Reference SelectReferencePlane(UIDocument doc)
        {
            Reference lineReference = doc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Reference Plane");
            return lineReference;
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
        /// // Filter walls that are parallel and perpendicular to the given reference.
        /// </summary>
        /// <returns>List of filtered walls.</returns>
        private static List<Wall> FilterWalls(List<Wall> walls, ReferencePlane reference, double toleranceAngle)
        {
            List<Wall> wallsFilter = new List<Wall>();

            // Get direction of reference plane
            double referenceX = Math.Abs(reference.Direction.X);
            double referenceY = Math.Abs(reference.Direction.Y);

            // Get lists of walls that are parallel and perpendicular
            List<Wall> wallsPar = new List<Wall>();
            List<Wall> wallsPer = new List<Wall>();
            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                // Checking if parallel
                if (Math.Abs(wallX - referenceX) <= toleranceAngle && Math.Abs(wallY - referenceY) <= toleranceAngle)
                    wallsPar.Add(wall);
                if (Math.Abs(wallX - referenceY) <= toleranceAngle && Math.Abs(wallY - referenceX) <= toleranceAngle)
                    wallsPer.Add(wall);
            }

            // Subtracting the lists from all walls list via set operation
            foreach (Wall wall in walls)
            {
                if (wallsPar.Contains(wall) | wallsPer.Contains(wall)) { }
                else
                    wallsFilter.Add(wall);
            }
            return wallsFilter;
        }

        private static void ShowResult(int count)
        {
            // Show result
            string text = (count == 0)
                ? "All walls are clear"
                : $"{count} walls added to Walls parallel filter";
            
            TaskDialog.Show("Walls parallel filter", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsParallel).Namespace + "." + nameof(WallsParallel);
        }
    }
}