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
        public static Reference GetLineRef(UIDocument doc)
        {
            Reference line_ref = doc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Lines"), "Select line");
            return line_ref;
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = false;
                options.View = view;

                // Get the line from user selection
                Selection sel = uidoc.Selection;
                IList<ElementId> selectedIds = uidoc.Selection.GetElementIds() as IList<ElementId>;
                List<Line> line = new List<Line>();
                if (selectedIds.Count == 0)
                {
                    // If no elements selected.
                    TaskDialog.Show("Section Annotations", "You haven't selected any elements.");
                    return Result.Failed;
                }
                else if (selectedIds.Count > 1)
                {
                    // If no elements selected.
                    TaskDialog.Show("Section Annotations", "Select only one vertical line.");
                    return Result.Failed;
                }
                else
                {
                    // If one element selected.
                    Element elem = doc.GetElement(selectedIds[0]);
                    if (elem.Category.Name != "Lines")
                    {
                        // If no line selected.
                        TaskDialog.Show("Section Annotations", "Select element is not a line.");
                        return Result.Failed;
                    }
                    else
                    {
                        foreach (Line l in elem.get_Geometry(options))
                        {
                            line.Add(l);
                        }
                        // Checking if not vertical
                        double d = line[0].Direction.Z;
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

                bool input_spots = data.result_spots;
                double input_thickness_cm = double.Parse(data.result_thickness);
                double input_thickness = UnitUtils.ConvertToInternalUnits(input_thickness_cm, DisplayUnitType.DUT_CENTIMETERS);
                double scale = view.Scale;
                double dim_value_moved_cm = 200; // If less then dimension segment text will be moved
                XYZ zero = new XYZ(0,0,0);
                int count = 0;
                int count_spots = 0;

                // Get Floors
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> floors_all = collector.OfClass(typeof(Floor))
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
                List<Floor> floors_thin = new List<Floor>();
                List<Floor> floors_thick = new List<Floor>();
                foreach (Floor f in floors_all)
                {
                    FloorType f_type = f.FloorType;

                    double f_thick = f_type.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                    if(f_thick > input_thickness)
                    {
                        floors_thick.Add(f);
                    }
                    else
                    {
                        floors_thin.Add(f);
                    }
                }

                IntersectionResultArray ira = new IntersectionResultArray();

                // List for intersection points
                List<Reference> intersections_thick_top = new List<Reference>();
                List<Reference> intersections_thick_bot = new List<Reference>();
                List<Face> intersection_faces_thick_top = new List<Face>();
                List<Face> intersection_faces_thick_bot = new List<Face>();

                // Iterate through floors solids and get faces
                foreach (Floor floor in floors_thick)
                {
                    foreach(Solid s in floor.get_Geometry(options))
                    {
                        foreach(Face face in s.Faces)
                        {
                            // Some faces are cylidrical so pass them
                            try
                            {
                                // Check if faces are horisontal
                                UV p = new UV(0,0);
                                if(Math.Round(face.ComputeNormal(p).X) == 0)
                                {
                                    if(Math.Round(face.ComputeNormal(p).Y) == 0)
                                    {
                                        if(Math.Round(face.ComputeNormal(p).Z) == 1)
                                        {
                                            SetComparisonResult intersection = face.Intersect(line[0], out ira);
                                            if(intersection == SetComparisonResult.Overlap)
                                            {
                                                intersections_thick_top.Add(face.Reference);
                                                intersection_faces_thick_top.Add(face);
                                            }
                                        }
                                        if (Math.Round(face.ComputeNormal(p).Z) == -1)
                                        {
                                            SetComparisonResult intersection = face.Intersect(line[0], out ira);
                                            if (intersection == SetComparisonResult.Overlap)
                                            {
                                                intersections_thick_bot.Add(face.Reference);
                                                intersection_faces_thick_bot.Add(face);
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                
                            }
                        }
                    }
                }

                // List for intersection points
                List<Reference> intersections_thin_top = new List<Reference>();
                List<Face> intersection_faces_thin_top = new List<Face>();

                // Iterate through floors solids and get faces
                foreach (Floor floor in floors_thin)
                {
                    foreach (Solid s in floor.get_Geometry(options))
                    {
                        foreach (Face face in s.Faces)
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
                                            SetComparisonResult intersection = face.Intersect(line[0], out ira);
                                            if (intersection == SetComparisonResult.Overlap)
                                            {
                                                intersections_thin_top.Add(face.Reference);
                                                intersection_faces_thin_top.Add(face);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }

                // Convert lists to ReferenceArrays
                ReferenceArray references = new ReferenceArray();

                foreach (Reference p in intersections_thick_top)
                {
                    references.Append(p);

                    count++;
                }
                foreach (Reference p in intersections_thick_bot)
                {
                    references.Append(p);

                    count++;
                }
                foreach (Reference p in intersections_thin_top)
                {
                    references.Append(p);

                    count++;
                }

                if(count > 0)
                {
                    count--;
                }

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Section Dimensions"))
                {
                    trans.Start();

                    if(input_spots)
                    {
                        for(int i = 0; i < intersections_thick_top.Count; i++)
                        {
                            double x = line[0].GetEndPoint(0).X;
                            double y = line[0].GetEndPoint(0).Y;
                            double z = intersection_faces_thick_top[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersections_thick_top[i], origin, zero, zero, origin, false);
                                count_spots++;
                            }
                            catch
                            {

                            }
                        }
                        for (int i = 0; i < intersections_thick_bot.Count; i++)
                        {
                            double x = line[0].GetEndPoint(0).X;
                            double y = line[0].GetEndPoint(0).Y;
                            double z = intersection_faces_thick_top[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersections_thick_bot[i], origin, zero, zero, origin, false);
                                count_spots++;
                            }
                            catch
                            {

                            }
                        }
                        for (int i = 0; i < intersections_thin_top.Count; i++)
                        {
                            double x = line[0].GetEndPoint(0).X;
                            double y = line[0].GetEndPoint(0).Y;
                            double z = intersection_faces_thick_top[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersections_thin_top[i], origin, zero, zero, origin, false);
                                count_spots++;
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Dimension d = doc.Create.NewDimension(view, line[0], references);

                            ElementTransformUtils.MoveElement(doc, d.Id, XYZ.BasisZ);
                            ElementTransformUtils.MoveElement(doc, d.Id, -XYZ.BasisZ);
                            doc.Regenerate();

                            // Remove leaders
                            d.get_Parameter(BuiltInParameter.DIM_LEADER).SetValueString("No");

                            ElementTransformUtils.MoveElement(doc, d.Id, XYZ.BasisZ);
                            ElementTransformUtils.MoveElement(doc, d.Id, -XYZ.BasisZ);
                            doc.Regenerate();

                            // Move little segments text
                            DimensionSegmentArray dsa = d.Segments;
                            foreach (DimensionSegment ds in dsa)
                            {
                                if (ds.IsTextPositionAdjustable())
                                {
                                    double value = UnitUtils.ConvertFromInternalUnits(ds.Value.Value, DisplayUnitType.DUT_CENTIMETERS);

                                    double ratio = 0.7; // Ratio of dimension text height to width
                                    if (value > 9)
                                        ratio = 1.5; // For 2-digit dimensions
                                    if (value > 99)
                                        ratio = 2.5; // For 3-digit dimensions

                                    double dim_size_d = d.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                                    double dim_size = UnitUtils.ConvertFromInternalUnits(dim_size_d, DisplayUnitType.DUT_CENTIMETERS) * ratio; // Size of the dimension along dimension line

                                    double factor = value / (scale * dim_size); // Factor calculated if dimension should be moved to the side
                                    
                                    if (factor < 1)
                                    {
                                        // Get the current text XYZ position
                                        XYZ currentTextPosition = ds.TextPosition;
                                        // Calculate moving offset
                                        double translation_z = UnitUtils.ConvertToInternalUnits((value + dim_size * scale) / 2 + 3, DisplayUnitType.DUT_CENTIMETERS);
                                        // Calculate a new XYZ position by transforming the current text position
                                        XYZ newTextPosition = Transform.CreateTranslation(new XYZ(0, 0, translation_z)).OfPoint(currentTextPosition);
                                        // Set the new text position for the segment's text
                                        ds.TextPosition = newTextPosition;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Section Annotations", ex.ToString());
                        }
                    }

                    trans.Commit();

                    if(count == 0)
                    {
                        TaskDialog.Show("Section Annotations", "No annotations created");
                    }
                    else
                    {
                        if(count_spots == 0)
                        {
                            TaskDialog.Show("Section Annotations", string.Format("Dimension with {0} segments created", count.ToString()));
                        }
                        else
                        {
                            TaskDialog.Show("Section Annotations", string.Format("{0} spot elevations created", count.ToString()));
                        }
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
