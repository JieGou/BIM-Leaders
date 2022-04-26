using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using System.Collections;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DimensionsPlanWalls : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get Application
            Application uiapp = doc.Application;

            double toleranceAngle = uiapp.AngleTolerance / 100; // 0.001 grad

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = doc.ActiveView
            };

            try
            {
                // Get the line from user selection
                Reference referenceLine = uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Lines"), "Select Line");
                DetailLine line = doc.GetElement(referenceLine) as DetailLine;
                if (line == null)
                {
                    TaskDialog.Show("Dimensions Plan Walls", "Wrong selection.");
                    return Result.Failed;
                }
                Line curve = line.GeometryCurve as Line;

                IEnumerable<Wall> walls = GetWallsStraight(doc);
                IEnumerable<Wall> walls_per = FilterWallsPer(toleranceAngle, curve, walls);
                IEnumerable<Reference> intersections_walls = FindIntersections(doc, curve, walls_per);

                IEnumerable<FamilyInstance> columns = GetColumns(doc);
                IEnumerable<FamilyInstance> columns_per = FilterColumnsPer(toleranceAngle, curve, columns);
                IEnumerable<Reference> intersections_columns = FindIntersections(doc, curve, columns_per);

                // Convert lists to ReferenceArray
                ReferenceArray references = new ReferenceArray();
                foreach (Reference i in intersections_walls)
                    references.Append(i);
                foreach (Reference i in intersections_columns)
                    references.Append(i);

                int count = references.Size - 1;

                using (Transaction trans = new Transaction(doc, "Dimension Plan Walls"))
                {
                    trans.Start();

                    doc.Create.NewDimension(doc.ActiveView, curve, references);

                    trans.Commit();
                }

                // Show result
                string text = (count == 0)
                    ? "Dimension creating error."
                    : $"Dimension with {count} segments was created.";
                TaskDialog.Show("Dimension Plan Walls", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get walls that have no dimension references on active view.
        /// </summary>
        /// <returns>List<ElementId> of walls.</returns>
        private static List<ElementId> GetWallIds(Document doc)
        {
            List<ElementId> wallIds = new List<ElementId>();

            // Get Walls
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            IEnumerable<Wall> wallsAll = collector.OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Wall>();

            // Get Dimensions
            FilteredElementCollector collector1 = new FilteredElementCollector(doc, doc.ActiveView.Id);
            IEnumerable<Dimension> dimensionsAll = collector1.OfClass(typeof(Dimension))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Dimension>();

            // Iterate walls
            foreach (Wall wall in wallsAll)
            {
                XYZ normalWall = new XYZ(0, 0, 0);
                try
                {
                    normalWall = wall.Orientation;
                }
                catch { continue; }
                // List for intersections count for each dimansion
                List<int> countIntersections = new List<int>();
                // Iterate dimensions
                foreach (Dimension dimension in dimensionsAll)
                {
                    Line dimensionLine = dimension.Curve as Line;
                    if (dimensionLine != null)
                    {
                        dimensionLine.MakeBound(0, 1);
                        XYZ point0 = dimensionLine.GetEndPoint(0);
                        XYZ point1 = dimensionLine.GetEndPoint(1);
                        XYZ dimensionLoc = point1.Subtract(point0).Normalize();
                        // Intersections count
                        int countIntersection = 0;

                        ReferenceArray referenceArray = dimension.References;
                        // Iterate dimension references
                        foreach (Reference reference in referenceArray)
                            if (reference.ElementId == wall.Id)
                                countIntersection++;
                        // If 2 dimensions on a wall so check if dimansion is parallel to wall normal
                        if (countIntersection >= 2)
                            if (Math.Round(Math.Abs((dimensionLoc.AngleTo(normalWall) / Math.PI - 0.5) * 2)) == 1) // Angle is from 0 to PI, so divide by PI - from 0 to 1, then...
                                countIntersections.Add(countIntersection);
                    }
                }
                // Check if no dimensions left
                if (countIntersections.Count == 0)
                    wallIds.Add(wall.Id);
            }
            return wallIds;
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
        /// Filter list of walls to get walls only perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the line.</returns>
        private static List<Wall> FilterWallsPer(double toleranceAngle, Line line, IEnumerable<Wall> walls)
        {
            List<Wall> wallsPer = new List<Wall>();

            // Get direction of reference plane
            double lineX = Math.Abs(line.Direction.X);
            double lineY = Math.Abs(line.Direction.Y);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(lineX) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineY) <= toleranceAngle))
                    wallsPer.Add(wall);
            }
            return wallsPer;
        }

        /// <summary>
        /// Get columns visible on active view.
        /// </summary>
        /// <returns>List of columns visible on active view.</returns>
        private static List<FamilyInstance> GetColumns(Document doc)
        {
            List<FamilyInstance> columns = new List<FamilyInstance>();

            // Get all structural columns on the view
            List<FamilyInstance> columns_str = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .ToElements()
                .Cast<FamilyInstance>()
                .ToList();
            // Get all columns on the view
            List<FamilyInstance> columns_arc = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
        private static List<FamilyInstance> FilterColumnsPer(double toleranceAngle, Line line, IEnumerable<FamilyInstance> columns)
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
                if (Math.Abs(col_angle) <= toleranceAngle)
                    columns_per.Add(i);
                else if (Math.Abs(col_angle - Math.PI) <= toleranceAngle)
                    columns_per.Add(i);
                // Checking if parallel
                else if (Math.Abs(col_angle - Math.PI / 2) <= toleranceAngle)
                    columns_per.Add(i);
            }
            return columns_per;
        }

        private static List<Reference> FindIntersections(Document doc, Line curve, IEnumerable<Element> elements)
        {
            List<Reference> intersections = new List<Reference>();

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = doc.ActiveView
            };

            foreach (Element e in elements)
            {
                foreach (Solid s in e.get_Geometry(opts))
                {
                    FaceArray faces = s.Faces;
                    foreach (PlanarFace face in faces)
                    {
                        // Check if faces are vertical
                        if (Math.Round(face.FaceNormal.Z) == 0)
                        {
                            SetComparisonResult intersection = face.Intersect(curve);
                            if (intersection == SetComparisonResult.Overlap)
                                intersections.Add(face.Reference);
                        }
                    }
                }
            }
            return intersections;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlanWalls).Namespace + "." + nameof(DimensionsPlanWalls);
        }
    }
}
