using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using System.Linq;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DimensionSectionFloors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = doc.ActiveView
            };

            // Get length units
#if VERSION2020

            DisplayUnitType units = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;

#elif VERSION2021

            ForgeTypeId units = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

#endif
            try
            {
                int count = 0;

                // Get the line from user selection
                IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;

                // Check and exit if wrong selection
                if (!ValidateLine(doc, options, selectedIds, out List<Line> lines).Any())
                    return Result.Failed;

                // Collector for data provided in window
                DimensionSectionFloorsForm form = new DimensionSectionFloorsForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DimensionSectionFloorsData data = form.DataContext as DimensionSectionFloorsData;

                bool inputSpots = data.ResultSpots;
                List<bool> inputPlacement = data.ResultPlacement;
                double inputThicknessCm = double.Parse(data.ResultThickness);

                double inputThickness = UnitUtils.ConvertToInternalUnits(inputThicknessCm, units); 

                // Get Floors
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> floorsAll = collector.OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .ToElements();

                // Divide Floors to thick and thin tor spot elevations arrangement
                List<Floor> floorsThin = new List<Floor>();
                List<Floor> floorsThick = new List<Floor>();
                foreach (Floor floor in floorsAll)
                {
                    FloorType floorType = floor.FloorType;

                    double floorThickness = floorType.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                    if(floorThickness > inputThickness)
                        floorsThick.Add(floor);
                    else
                        floorsThin.Add(floor);
                }

                // Lists for intersection points
                List<Face> intersectionFacesAll = new List<Face>();

                if (inputPlacement[0])
                {
                    List<Face> intersectionFacesThinTop = GetIntersections(options, lines[0], floorsThin)[0];
                    intersectionFacesAll.AddRange(intersectionFacesThinTop);
                }
                if (inputPlacement[1] || inputPlacement[2])
                {
                    List<List<Face>> intersectionFacesThick = GetIntersections(options, lines[0], floorsThick);

                    if (inputPlacement[1])
                    {
                        List<Face> intersectionFacesThickTop = intersectionFacesThick[0];
                        intersectionFacesAll.AddRange(intersectionFacesThickTop);
                    }
                    if (inputPlacement[2])
                    {
                        List<Face> intersectionFacesThickBot = intersectionFacesThick[1];
                        intersectionFacesAll.AddRange(intersectionFacesThickBot);
                    }
                }

                // Check if no intersections
                if (intersectionFacesAll.Count == 0)
                {
                    TaskDialog.Show("Section Annotations", "No intersections were found");
                    return Result.Failed;
                }

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Section Dimensions"))
                {
                    trans.Start();
                    
                    if (inputSpots)
                        count = CreateSpots(doc, lines[0], intersectionFacesAll);
                    else
                        count = CreateDimensions(doc, lines[0], intersectionFacesAll);
                    
                    trans.Commit();
                }

                // Result window
                string text = "";
                if (count == 0)
                    text = "No annotations created.";
                else
                    text = inputSpots
                        ? $"{count} spot elevations created."
                        : $"Dimension with {count} segments created.";
                TaskDialog.Show("Section Annotations", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Validate if selected line is single and vertical.
        /// </summary>
        /// <returns>List of lines with line in [0] index if OK. Null if failed.</returns>
        private static List<Line> ValidateLine(Document doc, Options options, IList<ElementId> selectedIds, out List<Line> lines)
        {
            lines = new List<Line>();

            // If no elements selected
            if (selectedIds.Count == 0)
                TaskDialog.Show("Section Annotations", "You haven't selected any lines.");
            // If more than 1 element is selected.
            else if (selectedIds.Count > 1)
                TaskDialog.Show("Section Annotations", "Select only one vertical line.");
            // If one element selected.
            else
            {
                Element element = doc.GetElement(selectedIds[0]);

                // If no line selected.
                if (element.Category.Name != "Lines")
                    TaskDialog.Show("Section Annotations", "Select element is not a line.");
                else
                {
                    foreach (Line l in element.get_Geometry(options))
                    {
                        // Checking if not vertical
                        double d = l.Direction.Z;
                        if (d != 1 && d != -1)
                            TaskDialog.Show("Section Annotations", "Selected line is not vertical.");
                        else
                            lines.Add(l);
                    } 
                }
            }
            return lines;
        }

        /// <summary>
        /// Get intersections from line and list of floors.
        /// </summary>
        /// <returns>List of floor faces that intersect with the given line.</returns>
        private static List<List<Face>> GetIntersections(Options options, Line line, List<Floor> floors)
        {
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
        /// <returns>Count of created elevation spots.</returns>
        private static int CreateSpots(Document doc, Line line, List<Face> intersectionFaces)
        {
            int count = 0;

            View view = doc.ActiveView; 
            XYZ zero = new XYZ(0, 0, 0);

            for (int i = 0; i < intersectionFaces.Count; i++)
            {
                double x = line.GetEndPoint(0).X;
                double y = line.GetEndPoint(0).Y;
                double z = intersectionFaces[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                XYZ origin = new XYZ(x, y, z);

                try
                {
                    SpotDimension sd = doc.Create.NewSpotElevation(view, intersectionFaces[i].Reference, origin, zero, zero, origin, false);
                    count++;
                }
                catch { }
            }

            return count;
        }

        /// <summary>
        /// Create dimension on a faces through a given line.
        /// </summary>
        /// <returns>Count of created dimension segments.</returns>
        private static int CreateDimensions(Document doc, Line line, List<Face> intersectionFaces)
        {
            int count = 0;

#if VERSION2020

            DisplayUnitType units = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;

#elif VERSION2021

            ForgeTypeId units = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();

#endif
            View view = doc.ActiveView;
            double scale = view.Scale;

            // Convert List<Face> to ReferenceArray
            ReferenceArray references = new ReferenceArray();
            foreach (Face face in intersectionFaces)
                references.Append(face.Reference);

            Dimension dimension = doc.Create.NewDimension(view, line, references);

            ElementTransformUtils.MoveElement(doc, dimension.Id, XYZ.BasisZ);
            ElementTransformUtils.MoveElement(doc, dimension.Id, -XYZ.BasisZ);
            doc.Regenerate();

            // Remove leaders
            dimension.get_Parameter(BuiltInParameter.DIM_LEADER).SetValueString("No");

            ElementTransformUtils.MoveElement(doc, dimension.Id, XYZ.BasisZ);
            ElementTransformUtils.MoveElement(doc, dimension.Id, -XYZ.BasisZ);
            doc.Regenerate();

            // Move little segments text
            DimensionSegmentArray dimensionSegments = dimension.Segments;
            foreach (DimensionSegment dimensionSegment in dimensionSegments)
            {
                count++;

                if (dimensionSegment.IsTextPositionAdjustable())
                {
                    double value = UnitUtils.ConvertFromInternalUnits(dimensionSegment.Value.Value, units);

                    double ratio = 0.7; // Ratio of dimension text height to width
                    if (value > 9)
                        ratio = 1.5; // For 2-digit dimensions
                    if (value > 99)
                        ratio = 2.5; // For 3-digit dimensions

                    double dimensionSizeD = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                    double dimensionSize = UnitUtils.ConvertFromInternalUnits(dimensionSizeD, units) * ratio; // Size of the dimension along dimension line

                    double factor = value / (scale * dimensionSize); // Factor calculated if dimension should be moved to the side

                    if (factor < 1)
                    {
                        // Get the current text XYZ position
                        XYZ currentTextPosition = dimensionSegment.TextPosition;
                        // Calculate moving offset
                        double translationZ = UnitUtils.ConvertToInternalUnits((value + dimensionSize * scale) / 2 + 3, units);
                        // Calculate a new XYZ position by transforming the current text position
                        XYZ newTextPosition = Transform.CreateTranslation(new XYZ(0, 0, translationZ)).OfPoint(currentTextPosition);
                        // Set the new text position for the segment's text
                        dimensionSegment.TextPosition = newTextPosition;
                    }
                }
            }
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
        }
    }
}
