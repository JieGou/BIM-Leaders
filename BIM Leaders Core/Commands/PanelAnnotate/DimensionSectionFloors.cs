using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloors : IExternalCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc = _uidoc.Document;
        private static DimensionSectionFloorsData _inputData;
        private static int _countSpots;
        private static int _countSegments;

        private const string TRANSACTION_NAME = "Annotate Section";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;

            try
            {
                if (CheckIfSectionIsSplit())
                    TaskDialog.Show(TRANSACTION_NAME, "Current view is a split section. Line may not lay in view plane.");

                // Get the line from user selection
                Reference referenceLine = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Lines"), "Select Line");
                DetailLine detailLine = _doc.GetElement(referenceLine) as DetailLine;
                if (detailLine == null)
                {
                    TaskDialog.Show(TRANSACTION_NAME, "Wrong selection.");
                    return Result.Failed;
                }
                Line line = detailLine.GeometryCurve as Line;

                // Check and exit if wrong selection
                if (!ValidateLine(line))
                    return Result.Failed;

                _inputData = GetUserInput();
                if (_inputData == null)
                    return Result.Cancelled;

                bool inputSpots = data.ResultSpots;
                bool inputPlacementThinTop = data.ResultPlacementThinTop;
                bool inputPlacementThickTop = data.ResultPlacementThickTop;
                bool inputPlacementThickBot = data.ResultPlacementThickBot;
                int inputThicknessCm = data.ResultThickness;

                GetDividedFloors(inputThicknessCm, out List <Floor> floorsThin, out List<Floor> floorsThick);
                List<Face> intersectionFacesAll = GetIntersectionFaces(line, floorsThin, floorsThick, inputPlacementThinTop, inputPlacementThickTop, inputPlacementThickBot);

                // Check if no intersections
                if (intersectionFacesAll.Count == 0)
                {
                    TaskDialog.Show(TRANSACTION_NAME, "No intersections were found");
                    return Result.Failed;
                }

                // Create annotations
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();
                    
                    if (inputSpots)
                        CreateSpots(line, intersectionFacesAll);
                    else
                        CreateDimensions(line, intersectionFacesAll);
                    
                    trans.Commit();
                }
                ShowResult(inputSpots);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static bool CheckIfSectionIsSplit()
        {
            bool result = false;
#if !VERSION2020
            ViewSection view = _doc.ActiveView as ViewSection;
            if (view.IsSplitSection())
                return true;
#endif
            return result;
        }

        /// <summary>
        /// Validate if selected line is single and vertical.
        /// </summary>
        /// <returns>True if OK. False if line is wrong.</returns>
        private static bool ValidateLine(Line line)
        {
            double direction = line.Direction.Z;
            if (direction != 1 && direction != -1)
            {
                TaskDialog.Show("Annotate Section", "Selected line is not vertical.");
                return false;
            }

            return true;
        }

        private static DimensionSectionFloorsData GetUserInput()
        {
            // Collector for data provided in window
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as DimensionSectionFloorsData;
        }

        /// <summary>
        /// Get floors in the current document, divided by the given thickness.
        /// </summary>
        /// <param name="thickness">Thickness value to divide floors into 2 lists.</param>
        /// <param name="floorsThin">Floors with thickness less than the given value.</param>
        /// <param name="floorsThick">Floors with thickness greater than the given value.</param>
        private static void GetDividedFloors(double thickness, out List<Floor> floorsThin, out List<Floor> floorsThick)
        {
            floorsThin = new List<Floor>();
            floorsThick = new List<Floor>();

            // Get length units
#if VERSION2020
            DisplayUnitType units = _doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
#else
            ForgeTypeId units = _doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
#endif

            double inputThickness = UnitUtils.ConvertToInternalUnits(thickness, units);

            // Get Floors
            IEnumerable<Element> floorsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .ToElements();

            // Divide Floors to thick and thin tor spot elevations arrangement
            foreach (Floor floor in floorsAll)
            {
                FloorType floorType = floor.FloorType;

                double floorThickness = floorType.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                if (floorThickness > inputThickness)
                    floorsThick.Add(floor);
                else
                    floorsThin.Add(floor);
            }
        }

        /// <summary>
        /// Get intersection faces of given floors lists for future dimensioning.
        /// </summary>
        /// <param name="floorsThin">First list of floors.</param>
        /// <param name="floorsThick">Second list of floors.</param>
        /// <param name="line">Line to find intersections.</param>
        /// <param name="inputPlacementThinTop">True if need to find intersections on top of the first floors list.</param>
        /// <param name="inputPlacementThickTop">True if need to find intersections on top of the second floors list.</param>
        /// <param name="inputPlacementThickBot">True if need to find intersections on bottom of the second floors list.</param>
        /// <returns></returns>
        private static List<Face> GetIntersectionFaces(Line line, List<Floor> floorsThin, List<Floor> floorsThick, bool inputPlacementThinTop, bool inputPlacementThickTop, bool inputPlacementThickBot)
        {
            List<Face> intersectionFacesAll = new List<Face>();

            if (inputPlacementThinTop)
            {
                List<Face> intersectionFacesThinTop = GetIntersections(line, floorsThin)[0];
                intersectionFacesAll.AddRange(intersectionFacesThinTop);
            }
            if (inputPlacementThickTop || inputPlacementThickBot)
            {
                List<List<Face>> intersectionFacesThick = GetIntersections(line, floorsThick);

                if (inputPlacementThickTop)
                {
                    List<Face> intersectionFacesThickTop = intersectionFacesThick[0];
                    intersectionFacesAll.AddRange(intersectionFacesThickTop);
                }
                if (inputPlacementThickBot)
                {
                    List<Face> intersectionFacesThickBot = intersectionFacesThick[1];
                    intersectionFacesAll.AddRange(intersectionFacesThickBot);
                }
            }

            return intersectionFacesAll;
        }

        /// <summary>
        /// Get intersections from line and list of floors.
        /// </summary>
        /// <returns>List of floor faces that intersect with the given line.</returns>
        private static List<List<Face>> GetIntersections(Line line, List<Floor> floors)
        {
            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = _doc.ActiveView
            };

            IntersectionResultArray ira = new IntersectionResultArray();

            // List for intersection points. Top[0], Bottom[1].
            List<Face> intersectionFaces0 = new List<Face>();
            List<Face> intersectionFaces1 = new List<Face>();
            List<List<Face>> intersectionFaces = new List<List<Face>>();
            intersectionFaces.Add(intersectionFaces0);
            intersectionFaces.Add(intersectionFaces1);

            // Iterate through floors solids and get faces
            foreach (Floor floor in floors)
            {
                foreach (Solid solid in floor.get_Geometry(options))
                {
                    foreach (Face face in solid.Faces)
                    {
                        // Some faces are cylidrical so pass them
                        try
                        {
                            // Check if faces are horizontal
                            UV p = new UV(0, 0);
                            if (Math.Round(face.ComputeNormal(p).X) == 0)
                            {
                                if (Math.Round(face.ComputeNormal(p).Y) == 0)
                                {
                                    if (Math.Round(face.ComputeNormal(p).Z) == 1)
                                    {
                                        SetComparisonResult intersection = face.Intersect(line, out ira);
                                        if (intersection == SetComparisonResult.Overlap)
                                            intersectionFaces[0].Add(face);
                                    }
                                    if (Math.Round(face.ComputeNormal(p).Z) == -1)
                                    {
                                        SetComparisonResult intersection = face.Intersect(line, out ira);
                                        if (intersection == SetComparisonResult.Overlap)
                                            intersectionFaces[1].Add(face);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            return intersectionFaces;
        }

        /// <summary>
        /// Create spot elevations on a faces through a given line.
        /// </summary>
        private static void CreateSpots(Line line, List<Face> intersectionFaces)
        {
            View view = _doc.ActiveView; 
            XYZ zero = new XYZ(0, 0, 0);

            for (int i = 0; i < intersectionFaces.Count; i++)
            {
                double x = line.GetEndPoint(0).X;
                double y = line.GetEndPoint(0).Y;
                double z = intersectionFaces[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                XYZ origin = new XYZ(x, y, z);

                try
                {
                    SpotDimension sd = _doc.Create.NewSpotElevation(view, intersectionFaces[i].Reference, origin, zero, zero, origin, false);
                    _countSpots++;
                }
                catch { }
            }
        }

        /// <summary>
        /// Create dimension on a faces through a given line.
        /// </summary>
        private static void CreateDimensions(Line line, List<Face> intersectionFaces)
        {
            // Convert List<Face> to ReferenceArray
            ReferenceArray references = new ReferenceArray();
            foreach (Face face in intersectionFaces)
                references.Append(face.Reference);

            Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, line, references);
            DimensionUtils.AdjustText(dimension);

#if !VERSION2020
            dimension.HasLeader = false;
#endif

            _countSegments += references.Size - 1;
        }

        private static void ShowResult(bool inputSpots)
        {
            // Result window
            string text = "";
            if (_countSpots == 0 && _countSegments == 0)
                text = "No annotations created.";
            else
                text = (inputSpots)
                    ? $"{_countSpots} spot elevations created."
                    : $"Dimension with {_countSegments} segments created.";

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
        }
    }
}
