using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using System.Windows.Controls;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionsPlanLine : IExternalCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static double _toleranceAngle;
        private static int _countDimensions;

        private const string TRANSACTION_NAME = "Dimension Plan Walls";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad

            try
            {
                // Get the line from user selection
                Reference referenceLine = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Lines"), "Select Line");
                DetailLine detailLine = _doc.GetElement(referenceLine) as DetailLine;
                if (detailLine == null)
                {
                    TaskDialog.Show(TRANSACTION_NAME, "Wrong selection.");
                    return Result.Failed;
                }

                Line line = detailLine.GeometryCurve as Line;
                ReferenceArray references = GetReferences(line);

                if (references.Size < 2)
                {
                    TaskDialog.Show(TRANSACTION_NAME, "Not enough numbrer references for dimension.");
                    return Result.Failed;
                }

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, line, references);
                    DimensionUtils.AdjustText(dimension);
#if !VERSION2020
                    dimension.HasLeader = false;
#endif
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

        /// <summary>
        /// Get ReferenceArray of elements that intersects the given line.
        /// </summary>
        /// <param name="doc">Current document.</param>
        /// <param name="line">Line to find the intersections.</param>
        /// <returns>ReferenceArray that can be used for dimension creating.</returns>
        private static ReferenceArray GetReferences(Line line)
        {
            ReferenceArray references = new ReferenceArray();

            IEnumerable<Wall> walls = GetWallsStraight();
            IEnumerable<Wall> wallsPer = FilterWallsPer(line, walls);
            IEnumerable<Reference> referencesWalls = FindIntersections(line, wallsPer);

            IEnumerable<FamilyInstance> columns = GetColumns();
            IEnumerable<FamilyInstance> columnsPer = FilterColumnsPer(line, columns);
            IEnumerable<Reference> referencesColumns = FindIntersections(line, columnsPer);

            // Convert lists to ReferenceArray
            foreach (Reference i in referencesWalls)
                references.Append(i);
            foreach (Reference i in referencesColumns)
                references.Append(i);

            _countDimensions = references.Size - 1;

            return references;
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
        /// Filter list of walls to get walls only perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the line.</returns>
        private static List<Wall> FilterWallsPer(Line line, IEnumerable<Wall> walls)
        {
            List<Wall> wallsPer = new List<Wall>();

            // Get direction of reference plane
            double lineX = Math.Abs(line.Direction.X);
            double lineY = Math.Abs(line.Direction.Y);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(lineX) <= _toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineY) <= _toleranceAngle))
                    wallsPer.Add(wall);
            }
            return wallsPer;
        }

        /// <summary>
        /// Get columns visible on active view.
        /// </summary>
        /// <returns>List of columns visible on active view.</returns>
        private static List<FamilyInstance> GetColumns()
        {
            List<FamilyInstance> columns = new List<FamilyInstance>();

            // Get all structural columns on the view
            List<FamilyInstance> columns_str = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();
            // Get all columns on the view
            List<FamilyInstance> columns_arc = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Columns)
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();

            columns.AddRange(columns_str);
            columns.AddRange(columns_arc);

            return columns;
        }

        /// <summary>
        /// Filter list of columns to get columns only parallel or perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of columns parallel or perpendicular to the line.</returns>
        private static List<FamilyInstance> FilterColumnsPer(Line line, IEnumerable<FamilyInstance> columns)
        {
            List<FamilyInstance> columns_per = new List<FamilyInstance>();

            foreach (FamilyInstance i in columns)
            {
                LocationPoint lp = i.Location as LocationPoint;

                double col_ox = Math.Cos(lp.Rotation);
                double col_oy = Math.Sin(lp.Rotation);
                XYZ col_xy = new XYZ(col_ox, col_oy, 0);

                double col_angle = col_xy.AngleTo(line.Direction);

                // Checking if parallel
                if (Math.Abs(col_angle) <= _toleranceAngle)
                    columns_per.Add(i);
                else if (Math.Abs(col_angle - Math.PI) <= _toleranceAngle)
                    columns_per.Add(i);
                // Checking if parallel
                else if (Math.Abs(col_angle - Math.PI / 2) <= _toleranceAngle)
                    columns_per.Add(i);
            }
            return columns_per;
        }

        private static List<Reference> FindIntersections(Line curve, IEnumerable<Element> elements)
        {
            List<Reference> intersections = new List<Reference>();

            foreach (Element element in elements)
            {
                List<Solid> solids = GetElementSolids(element);
                foreach (Solid solid in solids)
                    intersections.AddRange(FindIntersectionsWithSolid(solid, curve));
            }
            return intersections;
        }

        private static List<Solid> GetElementSolids(Element element)
        {
            List<Solid> solids = new List<Solid>();

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = element.Document.ActiveView
            };

            GeometryElement geometryElement = element.get_Geometry(opts);
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is GeometryInstance geometryInstance)
                {
                    FamilyInstance elementInstance = element as FamilyInstance;
                    Transform transform = elementInstance.GetTransform();

                    // Columns not dimension!
                    // Cause may be that we need take non-transformed geometry for reference...
                    // But we still need transformed one for intersections finding...
                    GeometryElement geometryElementInstance = geometryInstance.GetSymbolGeometry().GetTransformed(transform);
                    foreach (Solid solidInstance in geometryElementInstance)
                        if (solidInstance.Volume > 0)
                            solids.Add(solidInstance);
                }
                if (geometryObject is Solid solid)
                    if (solid.Volume > 0)
                        solids.Add(solid);
            }
            return solids;
        }

        private static List<Reference> FindIntersectionsWithSolid(Solid solid, Line line)
        {
            List<Reference> result = new List<Reference>();

            FaceArray faces = solid.Faces;
            foreach (PlanarFace face in faces)
            {
                // Check if faces are vertical
                if (Math.Round(face.FaceNormal.Z) == 0)
                {
                    SetComparisonResult intersection = face.Intersect(line);
                    if (intersection == SetComparisonResult.Overlap)
                        result.Add(face.Reference);
                }
            }
            return result;
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countDimensions == 0)
                ? "Dimension creating error."
                : $"Dimension with {_countDimensions} segments was created.";

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlanLine).Namespace + "." + nameof(DimensionsPlanLine);
        }
    }
}
