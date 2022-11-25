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
    public class DimensionsPlanM : BaseModel
    {
        private double _toleranceAngle;
        private int _countDimensions;
        private int _countSegments;

        #region PROPERTIES

        private double _searchStepCm;
        public double SearchStepCm
        {
            get { return _searchStepCm; }
            set
            {
                _searchStepCm = value;
                OnPropertyChanged(nameof(SearchStepCm));
            }
        }

        private double _searchStep;
        public double SearchStep
        {
            get { return _searchStep; }
            set
            {
                _searchStep = value;
                OnPropertyChanged(nameof(SearchStep));
            }
        }

        private double _searchDistanceCm;
        public double SearchDistanceCm
        {
            get { return _searchDistanceCm; }
            set
            {
                _searchDistanceCm = value;
                OnPropertyChanged(nameof(SearchDistanceCm));
            }
        }

        private double _searchDistance;
        public double SearchDistance
        {
            get { return _searchDistance; }
            set
            {
                _searchDistance = value;
                OnPropertyChanged(nameof(SearchDistance));
            }
        }

        private double _minReferences;
        public double MinReferences
        {
            get { return _minReferences; }
            set
            {
                _minReferences = value;
                OnPropertyChanged(nameof(MinReferences));
            }
        }

        #endregion

        public DimensionsPlanM(
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
            ConvertUserInput();

            // Collecting model elements to dimension
            List<Wall> wallsAll = GetWalls();
            List<FamilyInstance> columnsAll = GetColumns();

            // Get lists of walls and columns that are horizontal and vertical
            (List<Wall> wallsHor, List<Wall> wallsVer) = FilterWalls(wallsAll);
            List<FamilyInstance> columnsPer = FilterColumns(columnsAll);

            // Sum columns and walls
            IEnumerable<Element> elementsHor = wallsHor.Cast<Element>().Concat(columnsPer.Cast<Element>());
            IEnumerable<Element> elementsVer = wallsVer.Cast<Element>().Concat(columnsPer.Cast<Element>());

            // Get all faces that need a dimension
            // !!!  NEED ADJUSTMENT (LITTLE FACES FILTERING)
            List<PlanarFace> facesHorAll = GetFacesHorizontal(elementsHor);
            List<PlanarFace> facesVerAll = GetFacesVertical(elementsVer);

            // Storing data needed to create all dimensions
            Dictionary<Line, ReferenceArray> dimensionsDataHor = GetDimensionsData(facesHorAll, true);
            Dictionary<Line, ReferenceArray> dimensionsDataVer = GetDimensionsData(facesVerAll, false);

            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                foreach (KeyValuePair<Line, ReferenceArray> dimensionData in dimensionsDataHor)
                {
                    Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, dimensionData.Key, dimensionData.Value);
                    DimensionUtils.AdjustText(dimension);
#if !VERSION2020
                    dimension.HasLeader = false;
#endif
                    _countDimensions++;
                    _countSegments += dimensionData.Value.Size - 1;
                }
                foreach (KeyValuePair<Line, ReferenceArray> dimensionData in dimensionsDataVer)
                {
                    Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, dimensionData.Key, dimensionData.Value);
                    DimensionUtils.AdjustText(dimension);
#if !VERSION2020
                    dimension.HasLeader = false;
#endif
                    _countDimensions++;
                    _countSegments += dimensionData.Value.Size - 1;
                }

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        private void ConvertUserInput()
        {
#if VERSION2020
			SearchStep = UnitUtils.ConvertToInternalUnits(SearchStepCm, DisplayUnitType.DUT_CENTIMETERS);
			SearchDistance = UnitUtils.ConvertToInternalUnits(SearchDistanceCm, DisplayUnitType.DUT_CENTIMETERS);
#else
            SearchStep = UnitUtils.ConvertToInternalUnits(SearchStepCm, UnitTypeId.Centimeters);
            SearchDistance = UnitUtils.ConvertToInternalUnits(SearchDistanceCm, UnitTypeId.Centimeters);
#endif
        }

        /// <summary>
        /// Get list of straight walls visible on active view.
        /// </summary>
        /// <returns>List of straight walls visible on active view.</returns>
        private List<Wall> GetWalls()
        {
            List<Wall> walls = new List<Wall>();

            IEnumerable<Wall> wallsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Wall>();

            // Filter line walls
            foreach (Wall wall in wallsAll)
            {
                if (wall.Location is LocationCurve lc)
                    if (lc.Curve.GetType() == typeof(Line))
                        walls.Add(wall);
            }

            return walls;
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
        /// Filter list of walls to get walls only parallel or perpendicular to the given vector with the given angle tolerance.
        /// </summary>
        /// <returns>List of walls parallel or perpendicular to the vector.</returns>
        private (List<Wall>, List<Wall>) FilterWalls(IEnumerable<Wall> walls)
        {
            List<Wall> wallsPer = new List<Wall>();
            List<Wall> wallsPar = new List<Wall>();

            XYZ viewDirection = _doc.ActiveView.UpDirection;
            double lineX = Math.Abs(viewDirection.X);
            double lineY = Math.Abs(viewDirection.Y);

            foreach (Wall wall in walls)
            {
                double wallX = Math.Abs(wall.Orientation.X);
                double wallY = Math.Abs(wall.Orientation.Y);

                if ((Math.Abs(wallX) - Math.Abs(lineX) <= _toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineY) <= _toleranceAngle))
                    wallsPer.Add(wall);
                else if ((Math.Abs(wallX) - Math.Abs(lineY) <= _toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineX) <= _toleranceAngle))
                    wallsPar.Add(wall);
            }
            return (wallsPer, wallsPar);
        }

        private List<FamilyInstance> FilterColumns(IEnumerable<FamilyInstance> columns)
        {
            List<FamilyInstance> columnsPer = new List<FamilyInstance>();

            XYZ viewDirection = _doc.ActiveView.UpDirection;

            foreach (FamilyInstance column in columns)
            {
                if (!(column.Location is LocationPoint))
                    continue;

                LocationPoint lp = column.Location as LocationPoint;
                double columnX = Math.Cos(lp.Rotation);
                double columnY = Math.Sin(lp.Rotation);
                XYZ columnXY = new XYZ(columnX, columnY, 0);
                double columnAngle = columnXY.AngleTo(viewDirection);

                // Checking if parallel
                if (Math.Abs(columnAngle) <= _toleranceAngle)
                    columnsPer.Add(column);
                else if (Math.Abs(columnAngle - Math.PI) <= _toleranceAngle)
                    columnsPer.Add(column);
                // Checking if parallel
                else if (Math.Abs(columnAngle - Math.PI / 2) <= _toleranceAngle)
                    columnsPer.Add(column);
            }
            return columnsPer;
        }

        /// <summary>
        /// Get horizontal faces for dimensions.
        /// </summary>
        /// <returns>List of faces.</returns>
        private List<PlanarFace> GetFacesHorizontal(IEnumerable<Element> elements)
        {
            List<PlanarFace> facesAll = new List<PlanarFace>();

            View view = _doc.ActiveView;

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = view
            };

            foreach (Element element in elements)
            {
                foreach (Solid solid in element.get_Geometry(opts).OfType<Solid>())
                {
                    FaceArray faces = solid.Faces;
                    foreach (Face face in faces)
                    {
                        if (face is PlanarFace facePlanar)
                        {
                            // Check if face is vertical in 3D and horisontal on plan
                            double angleFaceToView = facePlanar.FaceNormal.AngleTo(view.UpDirection);
                            if (angleFaceToView <= _toleranceAngle || Math.Abs(angleFaceToView - Math.PI) <= _toleranceAngle)
                                facesAll.Add(facePlanar);
                        }
                    }
                }
            }
            return facesAll;
        }

        /// <summary>
        /// Get vertical faces for dimensions.
        /// </summary>
        /// <returns>List of faces.</returns>
        private List<PlanarFace> GetFacesVertical(IEnumerable<Element> elements)
        {
            List<PlanarFace> facesAll = new List<PlanarFace>();

            View view = _doc.ActiveView;

            Options opts = new Options()
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = view
            };

            foreach (Element element in elements)
            {
                foreach (Solid solid in element.get_Geometry(opts).OfType<Solid>())
                {
                    FaceArray faces = solid.Faces;
                    foreach (Face face in faces)
                    {
                        if (face is PlanarFace facePlanar)
                        {
                            // Check if face is vertical in 3D and horisontal on plan
                            double angleFaceToView = facePlanar.FaceNormal.AngleTo(view.UpDirection);
                            if (Math.Abs(angleFaceToView - Math.PI / 2) <= _toleranceAngle && (!(Math.Abs(facePlanar.FaceNormal.Z) == 1)))
                                facesAll.Add(facePlanar);
                        }
                    }
                }
            }
            return facesAll;
        }

        /// <summary>
        /// Get the data needed to create dimensions.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="isHorizontal">Are input faces horizontal.</param>
        private Dictionary<Line, ReferenceArray> GetDimensionsData(List<PlanarFace> faces, bool isHorizontal)
        {
            Dictionary<Line, ReferenceArray> dimensionsData = new Dictionary<Line, ReferenceArray>();

            // Faces buffer for faces with no dimension yet.
            List<PlanarFace> facesNotDimensioned = new List<PlanarFace>(faces);

            // Collector for all dimension lines and their faces.
            // This collector can be edited before converting to dimensionsData.
            Dictionary<Line, List<PlanarFace>> dimensionsDataCollector = new Dictionary<Line, List<PlanarFace>>();

            // Looping through faces
            int stopper = 1000;
            int stopperI = 0;
            while (facesNotDimensioned.Count > 1 && stopperI < stopper)
            {
                stopperI++;

                // Get the bottom or the left face
                PlanarFace faceCurrent;
                if (isHorizontal)
                    faceCurrent = FindFaceBottom(facesNotDimensioned);
                else
                    faceCurrent = FindFaceLeft(facesNotDimensioned);

                (Line maxIntersectionLine, List<PlanarFace> maxIntersectionFaces) = FindMaxIntersections(faceCurrent, faces);

                // Remove current face from buffer
                facesNotDimensioned.Remove(faceCurrent);

                // Line have not found any intersections
                if (maxIntersectionLine == null)
                    continue;

                // For the first face just add max to collected
                if (dimensionsDataCollector.Count == 0)
                    dimensionsDataCollector.Add(maxIntersectionLine, maxIntersectionFaces);
                else
                {
                    List<PlanarFace> maxIntersectionFacesPurged = PurgeFacesList(dimensionsDataCollector, maxIntersectionFaces);

                    // If after purging we still have new dimension
                    if (maxIntersectionFacesPurged != null)
                        dimensionsDataCollector.Add(maxIntersectionLine, maxIntersectionFacesPurged);
                }
            }

            // Convert all dimensions raw data (faces lists) to data with ReferenceArrays
            foreach (Line line in dimensionsDataCollector.Keys)
            {
                List<PlanarFace> facesToConvert = dimensionsDataCollector[line];

                ReferenceArray references = new ReferenceArray();
                foreach (PlanarFace face in facesToConvert)
                {
                    references.Append(face.Reference);
                }
                dimensionsData.Add(line, references);
            }

            return dimensionsData;
        }

        /// <summary>
        /// Find the most bottom (minimal Y coordinate) face on the view.
        /// </summary>
        /// <returns>Face.</returns>
        private PlanarFace FindFaceBottom(List<PlanarFace> faces)
        {
            // Select first face as default
            PlanarFace faceMin = faces.First();
            double faceMinY = faceMin.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Y;

            // Find face with minimal coordinate
            foreach (PlanarFace face in faces)
            {
                if (face.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Y < faceMinY)
                    faceMin = face;
            }

            return faceMin;
        }

        /// <summary>
        /// Find the most left (minimal X coordinate) face on the view.
        /// </summary>
        /// <returns>Face.</returns>
        private PlanarFace FindFaceLeft(List<PlanarFace> faces)
        {
            // Select first face as default
            PlanarFace faceMin = faces.First();
            double faceMinX = faceMin.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X;

            // Find face with minimal coordinate
            foreach (PlanarFace face in faces)
            {
                if (face.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X < faceMinX)
                    faceMin = face;
            }
            return faceMin;
        }

        /// <summary>
        /// Find the best place where to put a dimension on a face. Method analyzes faces list and finds where is the maximum number of intersections.
        /// </summary>
        /// <param name="face">Input face.</param>
        /// <returns></returns>
        private (Line, List<PlanarFace>) FindMaxIntersections(PlanarFace face, List<PlanarFace> faces)
        {
            Line maxIntersectionLine = null;
            List<PlanarFace> maxIntersectionFaces = new List<PlanarFace>();

            // Get the face farest points.
            (XYZ faceCurrentPointA, XYZ faceCurrentPointB) = FindFacePoints(face);

            // Check if face is horizontal
            View view = _doc.ActiveView;
            ViewPlan viewPlan = view as ViewPlan;
            double viewHeight = viewPlan.GenLevel.ProjectElevation + viewPlan.GetViewRange().GetOffset(PlanViewPlane.CutPlane);
            double angleFaceToView = face.FaceNormal.AngleTo(view.UpDirection);
            bool faceIsHosizontal = angleFaceToView <= _toleranceAngle || Math.Abs(angleFaceToView - Math.PI) <= _toleranceAngle;

            // Iterate through space between two points
            int maxIntersectionCount = 0;

            double lengthPast = 0;
            double lengthAll = faceCurrentPointA.DistanceTo(faceCurrentPointB);
            while (lengthPast < lengthAll)
            {
                // Make two points, with coordinates from face beginning
                // Search distance added and multiplied to extend the line for intersections search
                // lengthPast added in Y only, to move the line across the face
                XYZ point1;
                XYZ point2;
                if (faceIsHosizontal)
                {
                    point1 = new XYZ(faceCurrentPointA.X + lengthPast, faceCurrentPointA.Y + (SearchDistance * view.UpDirection.Y), viewHeight);
                    point2 = new XYZ(faceCurrentPointA.X + lengthPast, faceCurrentPointA.Y - (SearchDistance * view.UpDirection.Y), viewHeight);
                }
                else
                {
                    point1 = new XYZ(faceCurrentPointA.X - (SearchDistance * view.UpDirection.Y), faceCurrentPointA.Y + lengthPast, viewHeight);
                    point2 = new XYZ(faceCurrentPointA.X + (SearchDistance * view.UpDirection.Y), faceCurrentPointA.Y + lengthPast, viewHeight);
                }

                Line currentIntersectionLine = Line.CreateBound(point1, point2);

                // Find faces and their refs that intersect with the line
                List<PlanarFace> currentIntersectionFaces = FindIntersections(currentIntersectionLine, faces);
                //(List<Face> currentIntersectionFaces, ReferenceArray currentIntersectionRefs) = FindIntersections(currentIntersectionLine, facesNotDimensioned);

                lengthPast += SearchStep;

                if (currentIntersectionFaces.Count == 0)
                    continue;

                if (currentIntersectionFaces.Count > maxIntersectionCount)
                {
                    maxIntersectionFaces = currentIntersectionFaces;
                    maxIntersectionCount = maxIntersectionFaces.Count;
                    maxIntersectionLine = currentIntersectionLine;
                }
            }

            return (maxIntersectionLine, maxIntersectionFaces);
        }

        /// <summary>
        /// Find the farest points on the face.
        /// </summary>
        /// <returns>Tuple of two points.</returns>
        private (XYZ, XYZ) FindFacePoints(Face face)
        {
            // Find farest two points of the face
            List<XYZ> facePoints = face.GetEdgesAsCurveLoops()[0].Select(x => x.GetEndPoint(0)).ToList(); // Get face points
            XYZ facePointMin = facePoints[0];
            XYZ facePointMax = facePoints[1];

            foreach (XYZ point in facePoints)
            {
                if (point.X < facePointMin.X || point.Y < facePointMin.Y)
                    facePointMin = point;
                else if (point.X > facePointMax.X || point.Y > facePointMin.Y)
                    facePointMax = point;
            }

            return (facePointMin, facePointMax);
        }

        /// <summary>
        /// Find faces and its references that intersects with the curve.
        /// </summary>
        /// <returns>List of faces that intersect the given curve.</returns>
        private List<PlanarFace> FindIntersections(Curve curve, List<PlanarFace> faces)
        {
            List<PlanarFace> facesIntersected = new List<PlanarFace>();

            // Iterate through faces and get references
            foreach (PlanarFace face in faces)
            {
                SetComparisonResult intersection = face.Intersect(curve);
                if (intersection == SetComparisonResult.Overlap)
                    if (!facesIntersected.Contains(face))
                        facesIntersected.Add(face);
            }

            if (facesIntersected.Count < 2)
                facesIntersected.Clear();

            return facesIntersected;
        }

        /// <summary>
        /// We have to purge dimension line to prevent duplicate or almost the same dimension lines. Now it only return boolean as a flag, but in future need to add smart purging (if not all references are the same but part of them)
        /// </summary>
        /// <param name="facesCollectedData">Facescollected in previous iterations.</param>
        /// <param name="facesNew">New faces.</param>
        /// <param name="minUniqueReferences">If reference array will contain less unique references, it will be deleted with transfering references to existing array in the data.</param>
        /// <returns>Purged list of faces or null if new dimension not needed (its completely inside of collected one or almost inside - then add new items to the collected list).
        /// Can be improved later, to clear duplicates smartly.</returns>
        private List<PlanarFace> PurgeFacesList(Dictionary<Line, List<PlanarFace>> dimensionsDataCollector, List<PlanarFace> facesNew)
        {
            List<PlanarFace> facesNewPurged = new List<PlanarFace>();

            // From collected faces data and new faces list we make data with replaced: list of faces => tuples of 3 faces lists (see below)
            // dimensionsDataCollector + facesNew => facesDividedData(facesUniqueCollected, facesShared, facesUniqueNew)
            // { | ; ([C], [S], [N]) }   C - collected unique, S - shared, N - new unique.
            Dictionary<Line, Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>>> facesDividedData = new Dictionary<Line, Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>>>();

            // Go through collected lists and compare each one with the new list.
            foreach (Line line in dimensionsDataCollector.Keys)
            {
                List<PlanarFace> facesCollected = dimensionsDataCollector[line];

                // Recombine 2 faces lists into 3 faces lists (2 unique for each faces lists and 1 shared)
                // facesCollected + facesNew => facesUniqueNew + facesUniqueCollected + facesShared
                List<PlanarFace> facesUniqueNew = new List<PlanarFace>();
                List<PlanarFace> facesUniqueCollected = new List<PlanarFace>();
                List<PlanarFace> facesShared = new List<PlanarFace>();
                foreach (PlanarFace faceNew in facesNew)
                {
                    if (!facesCollected.Contains(faceNew))
                        facesUniqueNew.Add(faceNew);
                    else
                        facesShared.Add(faceNew);
                }
                foreach (PlanarFace faceCollected in facesCollected)
                {
                    if (!facesNew.Contains(faceCollected))
                        facesUniqueCollected.Add(faceCollected);
                }

                // If new faces list contains only already existing elements in one of the collected faces list.
                if (facesUniqueNew.Count == 0)
                    return null;

                Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>> facesTuples = new Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>>(facesUniqueCollected, facesShared, facesUniqueNew);
                facesDividedData.Add(line, facesTuples);
            }

            // Now analyze list of tuples

            // Find a new dimension with at least new faces.
            Line minimalNewKey = facesDividedData.First().Key;
            Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>> minimalNew = facesDividedData[minimalNewKey];
            foreach (Line line in facesDividedData.Keys)
            {
                Tuple<List<PlanarFace>, List<PlanarFace>, List<PlanarFace>> minimalNewCompare = facesDividedData[line];

                if (minimalNewCompare.Item3.Count < minimalNew.Item3.Count)
                    minimalNewKey = line;
            }

            // If minimal new faces count is less than threshold then add new dimension to this one with minimal.
            if (facesDividedData[minimalNewKey].Item3.Count < MinReferences)
            {
                dimensionsDataCollector[minimalNewKey].AddRange(facesDividedData[minimalNewKey].Item3);
                return null;
            }
            // Else make new dimension with shared and new faces.
            else
            {
                facesNewPurged.AddRange(facesDividedData[minimalNewKey].Item2);
                facesNewPurged.AddRange(facesDividedData[minimalNewKey].Item3);
            }

            return facesNewPurged;
        }

        private protected override string GetRunResult()
        {
            string text = "";

            text = (_countDimensions == 0)
                ? "Dimensions creating error."
                : $"{_countDimensions} dimensions with {_countSegments} segments were created.";

            return text;
        }
        #endregion
    }
}