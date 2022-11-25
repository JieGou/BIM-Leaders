using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class DimensionPlanLineM : BaseModel
    {
        private double _toleranceAngle;
        private int _countSegments;

        #region PROPERTIES

        private int _selectElements;
        public int SelectedElement
        {
            get { return _selectElements; }
            set
            {
                _selectElements = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        #endregion

        public DimensionPlanLineM(
            ExternalCommandData commandData,
            string transactionName,
            Action<string, RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction)
        {
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad
        }

        #region METHODS

        private protected override void TryExecute()
        {
            DetailLine detailLine = _doc.GetElement(new ElementId(SelectedElement)) as DetailLine;
            Line line = detailLine.GeometryCurve as Line;

            ReferenceArray references = GetReferences(line);

            if (references.Size < 2)
            {
                _result.Failed = true;
                _result.Result = "Not enough count of references for dimension.";
                return;
            }

            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, line, references);
                DimensionUtils.AdjustText(dimension);
#if !VERSION2020
                dimension.HasLeader = false;
#endif
                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        /// <summary>
        /// Get ReferenceArray of elements that intersects the given line.
        /// </summary>
        /// <param name="doc">Current document.</param>
        /// <param name="line">Line to find the intersections.</param>
        /// <returns>ReferenceArray that can be used for dimension creating.</returns>
        private ReferenceArray GetReferences(Line line)
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

            _countSegments = references.Size - 1;

            return references;
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
        /// Filter list of walls to get walls only perpendicular to the given line with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls perpendicular to the line.</returns>
        private List<Wall> FilterWallsPer(Line line, IEnumerable<Wall> walls)
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
        private List<FamilyInstance> GetColumns()
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
        private List<FamilyInstance> FilterColumnsPer(Line line, IEnumerable<FamilyInstance> columns)
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

        private List<Reference> FindIntersections(Line curve, IEnumerable<Element> elements)
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

        private List<Solid> GetElementSolids(Element element)
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
                    foreach (GeometryObject geometryObject2 in geometryElement)
                    {
                        if (geometryObject2 is Solid solid2)
                            if (solid2.Volume > 0)
                                solids.Add(solid2);
                    }
                }
                if (geometryObject is Solid solid)
                    if (solid.Volume > 0)
                        solids.Add(solid);
            }
            return solids;
        }

        private List<Reference> FindIntersectionsWithSolid(Solid solid, Line line)
        {
            List<Reference> result = new List<Reference>();

            FaceArray faces = solid.Faces;
            foreach (Face face in faces)
            {
                if (face is PlanarFace planarFace)
                    // Check if faces are vertical
                    if (Math.Round(planarFace.FaceNormal.Z) == 0)
                    {
                        SetComparisonResult intersection = planarFace.Intersect(line);
                        if (intersection == SetComparisonResult.Overlap)
                            result.Add(planarFace.Reference);
                    }
            }
            return result;
        }

        private protected override string GetRunResult()
        {
            string text = (_countSegments == 0)
                ? "Dimension creating error."
                : $"Dimension with {_countSegments} segments was created.";

            return text;
        }

        #endregion
    }
}