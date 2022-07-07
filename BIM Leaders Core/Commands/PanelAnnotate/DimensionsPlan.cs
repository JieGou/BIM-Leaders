using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.ApplicationServices;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DimensionsPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

			// Get View
			View view = doc.ActiveView;

			// Get Application
			Application uiapp = doc.Application;

            double toleranceAngle = uiapp.AngleTolerance / 100; // 0.001 grad

			int countDim = 0;
			int countRef = 0;

			try
            {
				// Get user provided information from window
				DimensionsPlanForm form = new DimensionsPlanForm();
				form.ShowDialog();

				if (form.DialogResult == false)
					return Result.Cancelled;

				// Collector for data provided in window
				DimensionsPlanData data = form.DataContext as DimensionsPlanData;

				// Getting input from user
				double searchStepCm = data.ResultSearchStep;
				double searchDistanceCm = data.ResultSearchDistance;

#if VERSION2020
				double searchStep = UnitUtils.ConvertToInternalUnits(searchStepCm, DisplayUnitType.DUT_CENTIMETERS);
				double searchDistance = UnitUtils.ConvertToInternalUnits(searchDistanceCm, DisplayUnitType.DUT_CENTIMETERS);
#else
                double searchStep = UnitUtils.ConvertToInternalUnits(searchStepCm, UnitTypeId.Centimeters);
				double searchDistance = UnitUtils.ConvertToInternalUnits(searchDistanceCm, UnitTypeId.Centimeters);
#endif
				// Collecting model elements to dimension
				List<Wall> wallsAll = GetWallsStraight(doc);
				List<FamilyInstance> columnsAll = GetColumns(doc);

				// Get lists of walls and columns that are horizontal and vertical
				(List<Wall> wallsHor, List<Wall> wallsVer) = FilterWallsPer(toleranceAngle, view.UpDirection, wallsAll);
				List<FamilyInstance> columnsPer = FilterColumnsPer(toleranceAngle, view.UpDirection, columnsAll);

				// Sum columns and walls
				IEnumerable<Element> elementsHor = wallsHor.Cast<Element>().Concat(columnsPer.Cast<Element>());
				IEnumerable<Element> elementsVer = wallsVer.Cast<Element>().Concat(columnsPer.Cast<Element>());

				// Get all horizontal faces that need a dimension
				// !!!  NEED ADJUSTMENT (LITTLE FACES FILTERING)
				List<Face> facesHorAll = GetFacesHorizontal(doc, toleranceAngle, elementsHor);
				List<Face> facesVerAll = GetFacesVertical(doc, toleranceAngle, elementsVer);

				// Storing data needed to create all dimensions
				Dictionary<Line, ReferenceArray> dimensionsDataHor = new Dictionary<Line, ReferenceArray>();
				Dictionary<Line, ReferenceArray> dimensionsDataVer = new Dictionary<Line, ReferenceArray>();
				GetDimensionsData(doc, dimensionsDataHor, facesHorAll, searchDistance, searchStep, true);
				GetDimensionsData(doc, dimensionsDataVer, facesVerAll, searchDistance, searchStep, false);

				using (Transaction trans = new Transaction(doc, "Dimension Plan Walls"))
                {
                    trans.Start();

					foreach (KeyValuePair<Line, ReferenceArray> dimensionData in dimensionsDataHor)
                    {
                        Dimension dimension = doc.Create.NewDimension(view, dimensionData.Key, dimensionData.Value);
						countDim++;
						countRef += (dimensionData.Value.Size - 1);
					}
					foreach (KeyValuePair<Line, ReferenceArray> dimensionData in dimensionsDataVer)
					{
						Dimension dimension = doc.Create.NewDimension(view, dimensionData.Key, dimensionData.Value);
						countDim++;
						countRef += (dimensionData.Value.Size - 1);
					}

					trans.Commit();
                }

				// Show result
				string text = (countDim == 0)
                    ? "Dimensions creating error."
                    : $"{countDim} dimensions with {countRef} segments were created.";
                TaskDialog.Show("Dimension Plan", text);
				
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
		/// Filter list of walls to get walls only parallel or perpendicular to the given vector with the given angle tolerance.
		/// </summary>
		/// <returns>List of walls parallel or perpendicular to the vector.</returns>
		private static (List<Wall>, List<Wall>) FilterWallsPer(double toleranceAngle, XYZ viewDirection, IEnumerable<Wall> walls)
		{
			List<Wall> wallsPer = new List<Wall>();
			List<Wall> wallsPar = new List<Wall>();

			double lineX = Math.Abs(viewDirection.X);
			double lineY = Math.Abs(viewDirection.Y);

			foreach (Wall wall in walls)
			{
				double wallX = Math.Abs(wall.Orientation.X);
				double wallY = Math.Abs(wall.Orientation.Y);

				if ((Math.Abs(wallX) - Math.Abs(lineX) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineY) <= toleranceAngle))
					wallsPer.Add(wall);
				if ((Math.Abs(wallX) - Math.Abs(lineY) <= toleranceAngle) && (Math.Abs(wallY) - Math.Abs(lineX) <= toleranceAngle))
					wallsPar.Add(wall);
			}
			return (wallsPer, wallsPar);
		}

		private static List<FamilyInstance> FilterColumnsPer(double toleranceAngle, XYZ viewDirection, IEnumerable<FamilyInstance> columns)
        {
			List<FamilyInstance> columnsPer = new List<FamilyInstance>();

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
				if (Math.Abs(columnAngle) <= toleranceAngle)
					columnsPer.Add(column);
				else if (Math.Abs(columnAngle - Math.PI) <= toleranceAngle)
					columnsPer.Add(column);
				// Checking if parallel
				else if (Math.Abs(columnAngle - Math.PI / 2) <= toleranceAngle)
					columnsPer.Add(column);
			}
			return columnsPer;
		}

		/// <summary>
		/// Get horizontal faces for dimensions.
		/// </summary>
		/// <returns>List of faces.</returns>
		private static List<Face> GetFacesHorizontal(Document doc, double toleranceAngle, IEnumerable<Element> elements)
        {
			List<Face> facesAll = new List<Face>();

			View view = doc.ActiveView;

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
							if (angleFaceToView <= toleranceAngle || Math.Abs(angleFaceToView - Math.PI) <= toleranceAngle)
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
		private static List<Face> GetFacesVertical(Document doc, double toleranceAngle, IEnumerable<Element> elements)
		{
			List<Face> facesAll = new List<Face>();

			View view = doc.ActiveView;

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
							if (Math.Abs(angleFaceToView - Math.PI / 2) <= toleranceAngle && (!(Math.Abs(facePlanar.FaceNormal.Z) == 1)))
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
		/// <param name="dimensionsData">Dictionary that will be filled after method is run.</param>
		/// <param name="isHorizontal">Are input faces horizontal.</param>
		private static void GetDimensionsData(Document doc, Dictionary<Line, ReferenceArray> dimensionsData, List<Face> faces, double searchDistance, double searchStep, bool isHorizontal)
        {
			View view = doc.ActiveView;
			ViewPlan viewPlan = view as ViewPlan;
			double viewHeight = viewPlan.GenLevel.ProjectElevation + viewPlan.GetViewRange().GetOffset(PlanViewPlane.CutPlane);

			// Faces buffer for faces with no dimension yet
			List<Face> facesNotDimensioned = new List<Face>(faces);

			// Looping through faces
			int stopper = 1000;
			int stopperI = 0;
			while (facesNotDimensioned.Count > 1 && stopperI < stopper)
			{
				stopperI++;

				Face faceCurrent;
				if (isHorizontal)
					faceCurrent = FindFaceBottom(facesNotDimensioned);
                else
					faceCurrent = FindFaceLeft(facesNotDimensioned);
				
				(XYZ faceCurrentPointA, XYZ faceCurrentPointB) = FindFacePoints(faceCurrent);

				// Iterate through space between two points of face
				// Find max number of intersections
				List<Face> maxIntersectionFaces = new List<Face>();
				ReferenceArray maxIntersectionRefs = new ReferenceArray();
				Line maxIntersectionLine = null;
				int maxIntersectionCount = 0;

				//Line lineMax = null;

				double lengthPast = 0;
				double lengthAll = faceCurrentPointA.DistanceTo(faceCurrentPointB);
				while (lengthPast < lengthAll)
				{
					// Make two points, with coordinates from face beginning
					// Search distance added and multiplied to extend the line for intersections search
					// lengthPast added in Y only, to move the line across the face
					XYZ point1;
					XYZ point2;
					if (isHorizontal)
                    {
						point1 = new XYZ(faceCurrentPointA.X + lengthPast, faceCurrentPointA.Y + searchDistance * view.UpDirection.Y, viewHeight);
						point2 = new XYZ(faceCurrentPointA.X + lengthPast, faceCurrentPointA.Y - searchDistance * view.UpDirection.Y, viewHeight);
					}
                    else
                    {
						point1 = new XYZ(faceCurrentPointA.X - searchDistance * view.UpDirection.Y, faceCurrentPointA.Y + lengthPast, viewHeight);
						point2 = new XYZ(faceCurrentPointA.X + searchDistance * view.UpDirection.Y, faceCurrentPointA.Y + lengthPast, viewHeight);
					}
					
					Line currentIntersectionLine = Line.CreateBound(point1, point2);

					// Find faces and their refs that intersect with the line
					(List<Face> currentIntersectionFaces, ReferenceArray currentIntersectionRefs) = FindIntersections(currentIntersectionLine, faces);
					//(List<Face> currentIntersectionFaces, ReferenceArray currentIntersectionRefs) = FindIntersections(currentIntersectionLine, facesNotDimensioned);

					lengthPast += searchStep;

					if (currentIntersectionRefs.Size == 0)
						continue;

					if (currentIntersectionRefs.Size > maxIntersectionCount)
					{
						maxIntersectionRefs = currentIntersectionRefs;
						maxIntersectionFaces = currentIntersectionFaces;
						maxIntersectionLine = currentIntersectionLine;
						maxIntersectionCount = maxIntersectionRefs.Size;
					}
				}

				// Line found some intersections
				if (maxIntersectionLine != null)
					if (PurgeIntersectionLine(dimensionsData, maxIntersectionRefs))
						dimensionsData.Add(maxIntersectionLine, maxIntersectionRefs);

				// Remove current face from buffer
				facesNotDimensioned.Remove(faceCurrent);

				// Remove dimensioned faces from buffer
				foreach (Face face in maxIntersectionFaces)
					facesNotDimensioned.Remove(face);
			}
		}

		/// <summary>
		/// Find the most bottom (minimal Y coordinate) face on the view.
		/// </summary>
		/// <returns>Face.</returns>
		private static Face FindFaceBottom(List<Face> faces)
        {
			// Select first face as default
			Face faceMin = faces.First();
			double faceMinY = faceMin.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Y;

			// Find face with minimal coordinate
			foreach (Face face in faces)
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
		private static Face FindFaceLeft(List<Face> faces)
		{
			// Select first face as default
			Face faceMin = faces.First();
			double faceMinX = faceMin.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X;

			// Find face with minimal coordinate
			foreach (Face face in faces)
			{
				if (face.EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X < faceMinX)
					faceMin = face;
			}
			return faceMin;
		}

		/// <summary>
		/// Find the farest points on the face.
		/// </summary>
		/// <returns>Tuple of two points.</returns>
		private static (XYZ, XYZ) FindFacePoints(Face face)
		{
			// Find farest two points of the face
			List<XYZ> facePoints = face.GetEdgesAsCurveLoops()[0].Select(x => x.GetEndPoint(0)).ToList(); // Get face points
			XYZ facePointA = facePoints[0];
			XYZ facePointB = facePoints[1];

			XYZ normal = face.ComputeNormal(new UV(0, 0));

			if (Math.Abs(normal.Y) == 1)
				foreach (XYZ point in facePoints)
				{
					if (point.X < facePointA.X)
						facePointA = point;
					else if (point.X > facePointB.X)
						facePointB = point;
				}
			else if (Math.Abs(normal.X) == 1)
				foreach (XYZ point in facePoints)
				{
					if (point.Y < facePointA.Y)
						facePointA = point;
					else if (point.Y > facePointB.Y)
						facePointB = point;
				}
			else
				throw new Exception("FindFacePoints Error. Face is not vertical or horisontal.");

			return (facePointA, facePointB);
		}

		/// <summary>
		/// Find faces and its references that intersects with the curve.
		/// </summary>
		/// <returns>List of faces and their references that intersect the given curve.</returns>
		private static (List<Face>, ReferenceArray) FindIntersections(Curve curve, List<Face> faces)
        {
			List<Face> facesIntersected = new List<Face>();
			ReferenceArray references = new ReferenceArray();
			
			// Iterate through faces and get references
			foreach (Face face in faces)
            {
				SetComparisonResult intersection = face.Intersect(curve);
				if (intersection == SetComparisonResult.Overlap)
					if (!facesIntersected.Contains(face))
                    {
						facesIntersected.Add(face);
						references.Append(face.Reference);
					}	
			}

			if (references.Size < 2)
            {
				facesIntersected.Clear();
				references.Clear();
			}

			return (facesIntersected, references);
		}

		/// <summary>
		/// We have to purge dimension line to prevent duplicates. Now it only return boolean as a flag, but in future need to add smart purging (if not all references are the same but part of them)
		/// </summary>
		/// <param name="dimensionsData">Current data for dimensions creating.</param>
		/// <param name="maxIntersectionLine">Line to purge.</param>
		/// <param name="references">References that the Line intersects.</param>
		/// <returns>True if input line is Okay and is not repeating some other line in dimensionsData. Need to be improved later, to edit the input line and references ad clear duplicates smartly.</returns>
		private static bool PurgeIntersectionLine(Document doc, Dictionary<Line, ReferenceArray> dimensionsData, ReferenceArray references)
        {
			bool lineIsUnique = true;

			// If reference array will contain less unique references,
			// it will be deleted with transfering references to existing array in the data.
			int maxUniqueReferences = 5;

			// Get input ReferenceArray Ids

			List<string> referencesIds = new List<string>();
			foreach (Reference reference in references)
			{
				referencesIds.Add(reference.ConvertToStableRepresentation(doc));
			}

			// Check if the same (or almost the same) ReferenceArray is in the given data
			foreach (Line dimensionsDataKey in dimensionsData.Keys)
            {
				ReferenceArray dimensionsDataReferences = dimensionsData[dimensionsDataKey];

				List<string> dimensionsDataReferenceIds = new List<string>();
				foreach (Reference dimensionsDataReference in dimensionsDataReferences)
                {
					dimensionsDataReferenceIds.Add(dimensionsDataReference.ConvertToStableRepresentation(doc));
				}

				// Divide all references to 3 lists (2 unique for each ReferenceArray and 1 shared)
				List<string> referencesIdsUnique = new List<string>();
				List<string> referencesIdsShared = new List<string>();
				List<string> dimensionsDataReferenceIdsUnique = new List<string>();
				foreach (string referencesId in referencesIds)
                {
					if (!dimensionsDataReferenceIds.Contains(referencesId))
						referencesIdsUnique.Add(referencesId);
					else
						referencesIdsShared.Add(referencesId);
				}
				foreach (string dimensionsDataReferenceId in dimensionsDataReferenceIds)
				{
					if (!referencesIds.Contains(dimensionsDataReferenceId))
						dimensionsDataReferenceIdsUnique.Add(dimensionsDataReferenceId);
				}

				// If new array contains only already existing elements in the data
				if (referencesIdsUnique.Count == 0)
					return false;

				// If 2 arrays are not so different so join current array to existing in the data
				// Or new array completely contains the old one and even bigger
				if (referencesIdsUnique.Count + dimensionsDataReferenceIdsUnique.Count < maxUniqueReferences
					|| dimensionsDataReferenceIdsUnique.Count == 0)
                {
					foreach (string s in referencesIdsUnique)
						dimensionsDataReferences.Append(Reference.ParseFromStableRepresentation(doc, s));

					lineIsUnique = false;
				}
			}				

			return lineIsUnique;
        }

		public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlan).Namespace + "." + nameof(DimensionsPlan);
        }
    }
}
