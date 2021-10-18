using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Dimension_Section_Floors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            // Get length units
            DisplayUnitType units = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;

            try
            {
                Options options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = false,
                    View = view
                };

                // Get the line from user selection
                IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;
                List<Line> lines = new List<Line>();


                // Validating the input line
                if (selectedIds.Count == 0) // If no elements selected
                {
                    TaskDialog.Show("Section Annotations", "You haven't selected any lines.");
                    return Result.Failed;
                }
                else if (selectedIds.Count > 1) // If more than 1 element is selected.
                {
                    TaskDialog.Show("Section Annotations", "Select only one vertical line.");
                    return Result.Failed;
                }
                else // If one element selected.
                {
                    Element element = doc.GetElement(selectedIds[0]);

                    // If no line selected.
                    if (element.Category.Name != "Lines")
                    {
                        TaskDialog.Show("Section Annotations", "Select element is not a line.");
                        return Result.Failed;
                    }
                    else
                    {
                        foreach (Line l in element.get_Geometry(options))
                            lines.Add(l);
                        // Checking if not vertical
                        double d = lines[0].Direction.Z;
                        if (d != 1 && d != -1)
                        {
                            TaskDialog.Show("Section Annotations", "Selected line is not vertical.");
                            return Result.Failed;
                        }
                    }
                }

                // Collector for data provided in window
                Dimension_Section_Floors_Data data = new Dimension_Section_Floors_Data();

                Dimension_Section_Floors_Form form = new Dimension_Section_Floors_Form();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Dimension_Section_Floors_Data;

                bool inputSpots = data.result_spots;
                double inputThicknessCm = double.Parse(data.result_thickness);
                double inputThickness = UnitUtils.ConvertToInternalUnits(inputThicknessCm, units);
                double scale = view.Scale;
                XYZ zero = new XYZ(0,0,0);
                int count = 0;
                int countSpots = 0;

                // Get Floors
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> floorsAll = collector.OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .ToElements();


                /*

                #  Transforming offset to world coordinates
                view_dir_x = view.ViewDirection.X
                view_dir_y = view.ViewDirection.Y
                offset_scaled_x = offset * view.Scale * view_dir_x
                offset_scaled_y = offset * view.Scale * view_dir_y
                off = XYZ(0 - offset_scaled_y, offset_scaled_x, 0)

                */


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

                IntersectionResultArray ira = new IntersectionResultArray();

                // List for intersection points
                List<Reference> intersectionsThickTop = new List<Reference>();
                List<Reference> intersectionsThickBot = new List<Reference>();
                List<Face> intersectionFacesThickTop = new List<Face>();
                List<Face> intersectionFacesThickBot = new List<Face>();

                // Iterate through floors solids and get faces
                foreach (Floor floor in floorsThick)
                {
                    foreach(Solid solid in floor.get_Geometry(options))
                    {
                        foreach(Face face in solid.Faces)
                        {
                            // Some faces are cylidrical so pass them
                            try
                            {
                                // Check if faces are horizontal
                                UV p = new UV(0,0);
                                if(Math.Round(face.ComputeNormal(p).X) == 0)
                                {
                                    if(Math.Round(face.ComputeNormal(p).Y) == 0)
                                    {
                                        if(Math.Round(face.ComputeNormal(p).Z) == 1)
                                        {
                                            SetComparisonResult intersection = face.Intersect(lines[0], out ira);
                                            if(intersection == SetComparisonResult.Overlap)
                                            {
                                                intersectionsThickTop.Add(face.Reference);
                                                intersectionFacesThickTop.Add(face);
                                            }
                                        }
                                        if (Math.Round(face.ComputeNormal(p).Z) == -1)
                                        {
                                            SetComparisonResult intersection = face.Intersect(lines[0], out ira);
                                            if (intersection == SetComparisonResult.Overlap)
                                            {
                                                intersectionsThickBot.Add(face.Reference);
                                                intersectionFacesThickBot.Add(face);
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                // List for intersection points
                List<Reference> intersectionsThinTop = new List<Reference>();
                List<Face> intersectionFacesThinTop = new List<Face>();

                // Iterate through floors solids and get faces
                foreach (Floor floor in floorsThin)
                {
                    foreach (Solid solid in floor.get_Geometry(options))
                    {
                        foreach (Face face in solid.Faces)
                        {
                            // Some faces are cylidrical so pass them
                            try
                            {
                                // Check if faces are horisontal
                                UV p = new UV(0, 0);
                                if (Math.Round(face.ComputeNormal(p).X) == 0)
                                {
                                    if (Math.Round(face.ComputeNormal(p).Y) == 0)
                                    {
                                        if (Math.Round(face.ComputeNormal(p).Z) == 1)
                                        {
                                            SetComparisonResult intersection = face.Intersect(lines[0], out ira);
                                            if (intersection == SetComparisonResult.Overlap)
                                            {
                                                intersectionsThinTop.Add(face.Reference);
                                                intersectionFacesThinTop.Add(face);
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

                // Convert lists to ReferenceArrays
                ReferenceArray references = new ReferenceArray();

                foreach (Reference reference in intersectionsThickTop)
                {
                    references.Append(reference);
                    count++;
                }
                foreach (Reference reference in intersectionsThickBot)
                {
                    references.Append(reference);
                    count++;
                }
                foreach (Reference reference in intersectionsThinTop)
                {
                    references.Append(reference);
                    count++;
                }

                if(count > 0)
                    count--;

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Section Dimensions"))
                {
                    trans.Start();

                    if (inputSpots)
                    {
                        for (int i = 0; i < intersectionsThickTop.Count; i++)
                        {
                            double x = lines[0].GetEndPoint(0).X;
                            double y = lines[0].GetEndPoint(0).Y;
                            double z = intersectionFacesThickTop[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersectionsThickTop[i], origin, zero, zero, origin, false);
                                countSpots++;
                            }
                            catch { }
                        }
                        for (int i = 0; i < intersectionsThickBot.Count; i++)
                        {
                            double x = lines[0].GetEndPoint(0).X;
                            double y = lines[0].GetEndPoint(0).Y;
                            double z = intersectionFacesThickTop[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersectionsThickBot[i], origin, zero, zero, origin, false);
                                countSpots++;
                            }
                            catch { }
                        }
                        for (int i = 0; i < intersectionsThinTop.Count; i++)
                        {
                            double x = lines[0].GetEndPoint(0).X;
                            double y = lines[0].GetEndPoint(0).Y;
                            double z = intersectionFacesThickTop[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersectionsThinTop[i], origin, zero, zero, origin, false);
                                countSpots++;
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        try
                        {
                            Dimension dimension = doc.Create.NewDimension(view, lines[0], references);

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
                        }
                        catch (Exception ex) { TaskDialog.Show("Section Annotations", ex.ToString()); }
                    }

                    trans.Commit();

                    if(count == 0)
                        TaskDialog.Show("Section Annotations", "No annotations created");
                    else
                    {
                        if(countSpots == 0)
                            TaskDialog.Show("Section Annotations", string.Format("Dimension with {0} segments created", count.ToString()));
                        else
                            TaskDialog.Show("Section Annotations", string.Format("{0} spot elevations created", countSpots.ToString()));
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Dimension_Section_Floors).Namespace + "." + nameof(Dimension_Section_Floors);
        }
    }
}
