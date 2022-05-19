using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
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

			double countDim = 0;
			double countRef = 0;

			int stopper = 1000;

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
				double searchStepCm = double.Parse(data.ResultSearchStep);
				double searchDistanceCm = double.Parse(data.ResultSearchDistance);

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

				// Storing data needed to create all dimensions
				Dictionary<Line, ReferenceArray> dimensionsData = new Dictionary<Line, ReferenceArray>();

				// Get all horizontal faces that need a dimension
				// !!!  NEED ADJUSTMENT (LITTLE FACES FILTERING)
				List<Face> facesHorAll = GetFacesHorizontal(doc, toleranceAngle, elementsHor);

				// Faces buffer for faces with no dimension yet
				List<Face> facesHorNotDim = facesHorAll;

				// Looping through faces
				int stopperI = 0;
				while (facesHorNotDim.Count > 1 && stopperI < stopper)
                {
					stopperI++;

					Face faceMin = FindFaceBottom(facesHorNotDim);
					(XYZ faceMinA, XYZ faceMinB) = FindFacePoints(faceMin);

					// Iterate through space between two points of face
					// Get max number of intersections
					List<Face> facesMax = new List<Face>();
					ReferenceArray refsMax = new ReferenceArray();

					int intersectsMax = 0;
					
					Line lineMax = null;

					double lengthPast = 0;
					while (lengthPast < faceMinA.DistanceTo(faceMinB))
                    {
						// Make two points, with coordinates from face beginning
						// Search distance added and multiplied to extend the line for intersections search
						// lengthPast added in Y only, to move the line across the face
						XYZ point1 = new XYZ(faceMinA.X + lengthPast, faceMinA.Y + searchDistance * view.UpDirection.Y, 0);
						XYZ point2 = new XYZ(faceMinA.X + lengthPast, faceMinA.Y - searchDistance * view.UpDirection.Y, 0);
						Line line = Line.CreateBound(point1, point2);

						// Find faces and its refs that intersect with the line
						(List<Face> faceIter, ReferenceArray refsIter) = FindReferences(line, facesHorAll.ToList());

						lengthPast += searchStep;

						if (refsIter.Size == 0)
							continue;

						if (refsIter.Size > intersectsMax)
                        {
							refsMax = refsIter;
							facesMax = faceIter;
							intersectsMax = refsMax.Size;
							lineMax = line;
						}
					}
					if (lineMax != null)
                    {
						dimensionsData.Add(lineMax, refsMax);
						countDim++;
						countRef += refsMax.Size - 1;
					}
					// Wall is too short - no one line found no intersections
					else
						facesHorNotDim.Remove(faceMin);

					// Remove dimensioned faces from buffer facesNotDim
					foreach (Face face in facesMax)
						facesHorNotDim.Remove(face);
				}

				// Get all vertical faces that need a dimension
				// !!!  NEED ADJUSTMENT (LITTLE FACES FILTERING)
				List<Face> facesVerAll = GetFacesVertical(doc, toleranceAngle, elementsVer);

				// Faces buffer for faces with no dimension yet
				List<Face> facesVerNotDim = facesVerAll;

				// Looping through faces
				stopperI = 0;
				while (facesVerNotDim.Count > 1 && stopperI < stopper)
				{
					stopperI++;

					Face faceMin = FindFaceLeft(facesVerNotDim);
					(XYZ faceMinA, XYZ faceMinB) = FindFacePoints(faceMin);

					// Iterate through space between two points of face
					// Get max number of intersections
					int intersectsMax = 0;
					List<Face> facesMax = new List<Face>();
					ReferenceArray refsMax = new ReferenceArray();
					Line lineMax = null;

					double lengthPast = 0;
					while (lengthPast < faceMinA.DistanceTo(faceMinB))
					{
						// Make two points, with coordinates from face beginning
						// Search distance added and multiplied to extend the line for intersections search
						// lengthPast added in X only, to move the line across the face
						XYZ point_1 = new XYZ(faceMinA.X - searchDistance * view.UpDirection.Y, faceMinA.Y + lengthPast, 0);
						XYZ point_2 = new XYZ(faceMinA.X + searchDistance * view.UpDirection.Y, faceMinA.Y + lengthPast, 0);
						Line line = Line.CreateBound(point_1, point_2);

						// Find faces and its refs that intersect with the line
						(List<Face> faceIter, ReferenceArray refsIter) = FindReferences(line, facesVerAll.ToList());

						lengthPast += searchStep;

						if (refsIter.Size == 0)
							continue;

						if (refsIter.Size > intersectsMax)
						{
							refsMax = refsIter;
							facesMax = faceIter;
							intersectsMax = refsMax.Size;
							lineMax = line;
						}
					}
					if (lineMax != null)
                    {
						dimensionsData.Add(lineMax, refsMax);
						countDim++;
						countRef += refsMax.Size - 1;
					}
					// Wall is too short - no one line found no intersections
					else
						facesHorNotDim.Remove(faceMin);

					// Remove dimensioned faces from buffer facesNotDim
					foreach (Face face in facesMax)
						facesVerNotDim.Remove(face);
				}

				using (Transaction trans = new Transaction(doc, "Dimension Plan Walls"))
                {
                    trans.Start();

					foreach (KeyValuePair<Line, ReferenceArray> dimensionData in dimensionsData)
                    {
						Dimension dimension = doc.Create.NewDimension(view, dimensionData.Key, dimensionData.Value);
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
				View = doc.ActiveView
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
				View = doc.ActiveView
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
		private static (List<Face>, ReferenceArray) FindReferences(Curve curve, List<Face> faces)
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
			return (facesIntersected, references);
		}

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlan).Namespace + "." + nameof(DimensionsPlan);
        }
    }
}
