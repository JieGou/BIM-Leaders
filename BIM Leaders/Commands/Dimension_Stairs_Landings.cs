using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Dimension_Stairs_Landings : IExternalCommand
    {
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
                // Selecting all runs in the view
                IEnumerable<StairsLanding> landings_all = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StairsLandings)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsLanding>();

                int count = 0;
                int count_i = 0;
                int count_spots = 0;

                // Ffiltering for Model-In-Place
                List<StairsLanding> landings_unsorted = new List<StairsLanding>();
                foreach (StairsLanding l in landings_all)
                {
                    try
                    {
                        double elev = l.BaseElevation;
                        landings_unsorted.Add(l);
                    }
                    catch
                    {

                    }
                }

                // Sort landings in groups by coordinates, each group have landings with same locations but different heights
                double threshold_cm = 150;
                double threshold = UnitUtils.ConvertToInternalUnits(threshold_cm, DisplayUnitType.DUT_CENTIMETERS);

                List<List<StairsLanding>> landings_sorted = new List<List<StairsLanding>>();
                int i = 0;
                int j = 0;
                while (i < landings_unsorted.Count)
                {
                    // Get unsorted landing
                    StairsLanding landing_unsorted = landings_unsorted[i];
                    BoundingBoxXYZ bb_unsorted = landing_unsorted.get_BoundingBox(view);
                    double x_unsorted = (bb_unsorted.Max.X + bb_unsorted.Min.X) / 2;
                    double y_unsorted = (bb_unsorted.Max.Y + bb_unsorted.Min.Y) / 2;
                    j = 0;
                    while (j < landings_sorted.Count)
                    {
                        // Compare coordinates with sorted landings
                        StairsLanding landing_sorted = landings_sorted[j][0];
                        BoundingBoxXYZ bb_sorted = landing_sorted.get_BoundingBox(view);
                        double x_sorted = (bb_sorted.Max.X + bb_sorted.Min.X) / 2;
                        double y_sorted = (bb_sorted.Max.Y + bb_sorted.Min.Y) / 2;
                        double x_x = x_unsorted - x_sorted;
                        double y_y = y_unsorted - y_sorted;
                        double c = Math.Abs(Math.Sqrt(x_x * x_x + y_y * y_y));
                        if (c < threshold)
                        {
                            landings_sorted[j].Add(landing_unsorted);
                            i++;
                        }
                        j++;
                    }
                    // Adding first landing to sorted landings
                    if (!landings_sorted.SelectMany(f => f).Contains(landing_unsorted))
                    {
                        landings_sorted.Add(new List<StairsLanding>());
                        landings_sorted.Last().Add(landing_unsorted);
                        i++;
                    }
                }
                // Create lines for dimensions
                List<Line> lines = new List<Line>();
                for(int l = 0; l < landings_sorted.Count; l++)
                {
                    BoundingBoxXYZ bb = landings_sorted[l][0].get_BoundingBox(view);
                    double line_x = (bb.Max.X + bb.Min.X) / 2;
                    double line_y = (bb.Max.Y + bb.Min.Y) / 2;
                    XYZ p_1 = new XYZ(line_x, line_y, -100);
                    XYZ p_2 = new XYZ(line_x, line_y, 1000);
                    Line line = Line.CreateBound(p_1, p_2);
                    lines.Add(line);
                }

                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;
                options.View = view;

                // List for intersection points
                List<ReferenceArray> intersections = new List<ReferenceArray>();
                List<List<Face>> intersection_faces = new List<List<Face>>();

                // Iterate through landings solids and get references and faces
                for(i=0; i<landings_sorted.Count; i++)
                {
                    ReferenceArray intersections_i = new ReferenceArray();
                    List<Face> intersection_faces_i = new List<Face>();
                    foreach (StairsLanding l in landings_sorted[i])
                    {
                        foreach (Solid s in l.get_Geometry(options))
                        {
                            foreach (Face face in s.Faces)
                            {
                                // Some faces are curved so pass them
                                try
                                {
                                    // Check if faces are horisontal
                                    UV p = new UV(0, 0);
                                    XYZ normal = face.ComputeNormal(p);
                                    if (Math.Round(normal.X) == 0 && Math.Round(normal.Y) == 0)
                                    {
                                        if (Math.Round(normal.Z) == 1 || Math.Round(normal.Z) == -1)
                                        {
                                            intersections_i.Append(face.Reference);
                                            intersection_faces_i.Add(face);
                                            count_i++;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {

                                }
                            }
                        }
                    }
                    intersections.Add(intersections_i);
                    intersection_faces.Add(intersection_faces_i);
                }

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Dimension Runs"))
                {
                    trans.Start();

                    XYZ zero = new XYZ(0, 0, 0);

                    for (i=0; i<lines.Count; i++)
                    {
                        Dimension dimension = doc.Create.NewDimension(view, lines[i], intersections[i]);
                        count++;
                        for (j=0; j<intersections[i].Size; j++)
                        {
                            double x_1 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X;
                            double x_2 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(2).AsCurve().GetEndPoint(0).X;
                            double x = (x_1 + x_2) / 2;
                            double y_1 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Y;
                            double y_2 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(2).AsCurve().GetEndPoint(0).Y;
                            double y = (y_1 + y_2) / 2;
                            double z = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersection_faces[i][j].Reference, origin, zero, zero, origin, false);
                                count_spots++;
                            }
                            catch
                            {

                            }
                        }
                    }

                    trans.Commit();
                    string text = string.Format("{0} dimension lines with {1} references were created. {1} spot elevations were created.",
                        count.ToString(),
                        count_i.ToString());
                    if (count == 0 && count_i == 0)
                    {
                        TaskDialog.Show("Dimension Stairs", "No annotations created");
                    }
                    else
                    {
                        TaskDialog.Show("Dimension Stairs", text);
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
