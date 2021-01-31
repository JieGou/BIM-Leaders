using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Dimension_Section_Floors : IExternalCommand
    {
        public class LineSelectionFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                if (element.Category.Name == "Lines")
                {
                    return true;
                }
                return false;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }
        public static Reference GetLineRef(UIDocument doc)
        {
            ReferenceArray ra = new ReferenceArray();
            ISelectionFilter selFilter = new LineSelectionFilter();
            Reference line_ref = doc.Selection.PickObject(ObjectType.Element, selFilter, "Select line");
            return line_ref;
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                // Getting input from user
                bool input_spots = false;
                double input_thickness_cm = 10;
                using (Dimension_Section_Floors_Form form = new Dimension_Section_Floors_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        input_spots = form.Result_Spots();
                        input_thickness_cm = Decimal.ToDouble(form.Result_Thickness());
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }
                double input_thickness = UnitUtils.ConvertToInternalUnits(input_thickness_cm, DisplayUnitType.DUT_CENTIMETERS);

                XYZ zero = new XYZ(0,0,0);

                // Get Floors
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> floors_all = collector.OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .ToElements();

                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = false;
                options.View = view;

                Element line_element = doc.GetElement(GetLineRef(uidoc).ElementId);
                List<Line> line = new List<Line>();
                foreach (Line l in line_element.get_Geometry(options))
                {
                    line.Add(l);
                }

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
                int count = 0;
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

                int count_spots = 0;

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
                            doc.Create.NewDimension(view, line[0], references);
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
    }
}
