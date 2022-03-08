﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DimensionStairsLandings : IExternalCommand
    {
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
                // Selecting all runs in the view
                IEnumerable<StairsLanding> landingsAll = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StairsLandings)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsLanding>();

                int count = 0;
                int countIntersections = 0;
                int countSpots = 0;

                // Ffiltering for Model-In-Place
                List<StairsLanding> landingsUnsorted = new List<StairsLanding>();
                foreach (StairsLanding landing in landingsAll)
                {
                    try
                    {
                        double elevation = landing.BaseElevation;
                        landingsUnsorted.Add(landing);
                    }
                    catch { }
                }

                // Sort landings in groups by coordinates, each group have landings with same locations but different heights
                double thresholdCm = 150;
                double threshold = UnitUtils.ConvertToInternalUnits(thresholdCm, DisplayUnitType.DUT_CENTIMETERS);

                List<List<StairsLanding>> landingsSorted = new List<List<StairsLanding>>();
                int i = 0;
                int j = 0;
                while (i < landingsUnsorted.Count)
                {
                    // Get unsorted landing
                    StairsLanding landingUnsorted = landingsUnsorted[i];
                    BoundingBoxXYZ unsortedBB = landingUnsorted.get_BoundingBox(view);
                    double unsortedX = (unsortedBB.Max.X + unsortedBB.Min.X) / 2;
                    double unsortedY = (unsortedBB.Max.Y + unsortedBB.Min.Y) / 2;
                    j = 0;
                    while (j < landingsSorted.Count)
                    {
                        // Compare coordinates with sorted landings
                        StairsLanding landingSorted = landingsSorted[j][0];
                        BoundingBoxXYZ sortedBB = landingSorted.get_BoundingBox(view);
                        double sortedX = (sortedBB.Max.X + sortedBB.Min.X) / 2;
                        double sortedY = (sortedBB.Max.Y + sortedBB.Min.Y) / 2;
                        double distanceX = unsortedX - sortedX;
                        double distanceY = unsortedY - sortedY;
                        double distance = Math.Abs(Math.Sqrt(distanceX * distanceX + distanceY * distanceY));
                        if (distance < threshold)
                        {
                            landingsSorted[j].Add(landingUnsorted);
                            i++;
                        }
                        j++;
                    }
                    // Adding first landing to sorted landings
                    if (!landingsSorted.SelectMany(x => x).Contains(landingUnsorted))
                    {
                        landingsSorted.Add(new List<StairsLanding>());
                        landingsSorted.Last().Add(landingUnsorted);
                        i++;
                    }
                }
                // Create lines for dimensions
                List<Line> lines = new List<Line>();
                for(i = 0; i < landingsSorted.Count; i++)
                {
                    BoundingBoxXYZ bb = landingsSorted[i][0].get_BoundingBox(view);
                    double lineX = (bb.Max.X + bb.Min.X) / 2;
                    double lineY = (bb.Max.Y + bb.Min.Y) / 2;
                    XYZ point1 = new XYZ(lineX, lineY, -100);
                    XYZ point2 = new XYZ(lineX, lineY, 1000);
                    Line line = Line.CreateBound(point1, point2);
                    lines.Add(line);
                }

                Options options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = true,
                    View = view
                };

                // List for all intersection points
                List<ReferenceArray> intersections = new List<ReferenceArray>();
                List<List<Face>> intersectionFaces = new List<List<Face>>();

                // Iterate through landings solids and get references and faces
                for (i = 0; i < landingsSorted.Count; i++)
                {
                    ReferenceArray iIntersections = new ReferenceArray();
                    List<Face> iIntersectionFaces = new List<Face>();
                    foreach (StairsLanding landing in landingsSorted[i])
                    {
                        foreach (Solid solid in landing.get_Geometry(options))
                        {
                            foreach (Face face in solid.Faces)
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
                                            iIntersections.Append(face.Reference);
                                            iIntersectionFaces.Add(face);
                                            countIntersections++;
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    intersections.Add(iIntersections);
                    intersectionFaces.Add(iIntersectionFaces);
                }

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Dimension Runs"))
                {
                    trans.Start();

                    XYZ zero = new XYZ(0, 0, 0);

                    for (i = 0; i < lines.Count; i++)
                    {
                        Dimension dimension = doc.Create.NewDimension(view, lines[i], intersections[i]);
                        count++;
                        for (j = 0; j < intersections[i].Size; j++)
                        {
                            /*
                            double x_1 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).X;
                            double x_2 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(2).AsCurve().GetEndPoint(0).X;
                            double x = (x_1 + x_2) / 2;
                            double y_1 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Y;
                            double y_2 = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(2).AsCurve().GetEndPoint(0).Y;
                            double y = (y_1 + y_2) / 2;
                            double z = intersection_faces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            XYZ origin = new XYZ(x, y, z);
                            */

                            // Get the center of the face
                            //EdgeArray edges = intersection_faces[i][j].EdgeLoops.get_Item(0);
                            EdgeArray edges = intersectionFaces[i][0].EdgeLoops.get_Item(0);
                            double coordinatesTotalX = 0;
                            double coordinatesTotalY = 0;
                            // Average coordinates for X and Y
                            foreach (Edge edge in edges)
                            {
                                coordinatesTotalX += edge.AsCurve().GetEndPoint(0).X;
                                coordinatesTotalY += edge.AsCurve().GetEndPoint(0).Y;
                            }
                            double x = coordinatesTotalX / edges.Size;
                            double y = coordinatesTotalY / edges.Size;
                            // Z coordinate
                            double z = intersectionFaces[i][j].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                            // Center point
                            XYZ origin = new XYZ(x, y, z);

                            try
                            {
                                SpotDimension sd = doc.Create.NewSpotElevation(view, intersectionFaces[i][j].Reference, origin, zero, zero, origin, false);
                                countSpots++;
                            }
                            catch { }
                        }
                    }

                    trans.Commit();
                    string text = string.Format("{0} dimension lines with {1} references were created. {1} spot elevations were created.",
                        count.ToString(),
                        countIntersections.ToString());
                    if (count == 0 && countIntersections == 0)
                        TaskDialog.Show("Dimension Stairs", "No annotations created");
                    else
                        TaskDialog.Show("Dimension Stairs", text);
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
            return typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
        }
    }
}