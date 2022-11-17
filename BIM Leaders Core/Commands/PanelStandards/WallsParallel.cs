using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsParallel : BaseCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static double _toleranceAngle;
        private static int _countWallsFiltered;

        private const string FILTER_NAME = "Check - Walls parralel";
        private readonly Color FILTER_COLOR = new Color(255, 127, 39);

        public WallsParallel()
        {
            _transactionName = "Walls Parralel Check";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad.

            _runStarted = true;

            try
            {
                ReferencePlane reference = _doc.GetElement(SelectReferencePlane().ElementId) as ReferencePlane;

                List<Wall> walls = GetWallsStraight();
                ICollection<Element> wallsFilter = FilterWalls(walls, reference) as ICollection<Element>;

                if (wallsFilter.Count == 0)
                    return Result.Succeeded;

                using (Transaction trans = new Transaction(_doc, _transactionName))
                {
                    trans.Start();

                    Element filter = ViewFilterUtils.CreateSelectionFilter(_doc, FILTER_NAME, wallsFilter);
                    ViewFilterUtils.SetupFilter(_doc, filter, FILTER_COLOR);

                    trans.Commit();
                }

                _runResult = GetRunResult();
                ShowResult();
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult();
                return Result.Failed;
            }
        }

        /// <summary>
        /// Allow user to select an element of Reference Plane category.
        /// </summary>
        /// <returns>Reference as a result of user selection.</returns>
        private static Reference SelectReferencePlane()
        {
            Reference lineReference = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Reference Plane");
            return lineReference;
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
        private static List<Wall> FilterWalls(List<Wall> walls, ReferencePlane reference)
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
                if (Math.Abs(wallX - referenceX) <= _toleranceAngle && Math.Abs(wallY - referenceY) <= _toleranceAngle)
                    wallsPar.Add(wall);
                if (Math.Abs(wallX - referenceY) <= _toleranceAngle && Math.Abs(wallY - referenceX) <= _toleranceAngle)
                    wallsPer.Add(wall);
            }

            // Subtracting the lists from all walls list via set operation
            foreach (Wall wall in walls)
            {
                if (wallsPar.Contains(wall) | wallsPer.Contains(wall)) { }
                else
                    wallsFilter.Add(wall);
            }

            _countWallsFiltered = wallsFilter.Count;

            return wallsFilter;
        }

        private string GetRunResult()
        {
            string text = (_countWallsFiltered == 0)
                ? "All walls are clear"
                : $"{_countWallsFiltered} walls added to filter \"{FILTER_NAME}\".";

            return text;
        }

        private protected override async void Run(ExternalCommandData commandData) { return; }

        public static string GetPath()
        {
            return typeof(WallsParallel).Namespace + "." + nameof(WallsParallel);
        }
    }
}