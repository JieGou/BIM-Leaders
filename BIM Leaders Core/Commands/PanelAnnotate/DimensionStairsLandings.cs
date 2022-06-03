using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

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

            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                View = view
            };

            int countSpots = 0;
            int countDimensions = 0;

            try
            {
                DimensionStairsLandingsForm form = new DimensionStairsLandingsForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DimensionStairsLandingsData data = form.DataContext as DimensionStairsLandingsData;
                int thresholdCm = data.ResultDistance; // Threshold for sorting landings into lists. Each list contains landings located over each other.

#if VERSION2020
                double threshold = UnitUtils.ConvertToInternalUnits(thresholdCm, DisplayUnitType.DUT_CENTIMETERS);
#else
                double threshold = UnitUtils.ConvertToInternalUnits(thresholdCm, UnitTypeId.Centimeters);
#endif

                // Selecting all landings in the view
                List<StairsLanding> landingsUnsorted = new FilteredElementCollector(doc, view.Id)
                    .OfClass(typeof(StairsLanding))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsLanding>()
                    .ToList();

                List<List<StairsLanding>> landingsSorted = SortLandings(doc, threshold, landingsUnsorted);

                List<Line> lines = CalculateLines(doc, landingsSorted);

                List<List<Face>> intersectionFaces = GetIntersections(options, landingsSorted);

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Dimension Runs"))
                {
                    trans.Start();

                    countSpots = CreateSpots(doc, lines, intersectionFaces);
                    countDimensions = CreateDimensions(doc, lines, intersectionFaces);

                    trans.Commit();
                }

                // Show result
                string text = (countSpots == 0 && countDimensions == 0)
                    ? "No annotations created"
                    : $"{countSpots} spot elevations were created. {countDimensions} dimension lines were created.";
                TaskDialog.Show("Dimension Stairs", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Sort landings in groups by coordinates, each group have landings with same locations but different heights.
        /// </summary>
        /// <returns>List of landings.</returns>
        private static List<List<StairsLanding>> SortLandings(Document doc, double threshold, List<StairsLanding> landingsUnsorted)
        {
            List<List<StairsLanding>> landingsSorted = new List<List<StairsLanding>>();

            View view = doc.ActiveView;

            int i = 0;
            int j = 0;
            while (i < landingsUnsorted.Count)
            {
                // Get unsorted landing center
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
            return landingsSorted;
        }

        /// <summary>
        /// Create lines for dimensions.
        /// </summary>
        /// <returns>Lines list.</returns>
        private static List<Line> CalculateLines(Document doc, List<List<StairsLanding>> landingsSorted)
        {
            List<Line> lines = new List<Line>();

            View view = doc.ActiveView;

            for (int i = 0; i < landingsSorted.Count; i++)
            {
                BoundingBoxXYZ bb = landingsSorted[i][0].get_BoundingBox(view);
                double lineX = (bb.Max.X + bb.Min.X) / 2;
                double lineY = (bb.Max.Y + bb.Min.Y) / 2;
                XYZ point1 = new XYZ(lineX, lineY, -100);
                XYZ point2 = new XYZ(lineX, lineY, 1000);
                Line line = Line.CreateBound(point1, point2);
                lines.Add(line);
            }
            return lines;
        }

        /// <summary>
        /// List for all intersection points.
        /// </summary>
        /// <returns>List for all intersection points.</returns>
        private static List<List<Face>> GetIntersections(Options options, List<List<StairsLanding>> landingsSorted)
        {
            List<List<Face>> intersectionFaces = new List<List<Face>>();

            // Iterate through landings solids and get references and faces
            for (int i = 0; i < landingsSorted.Count; i++)
            {
                //ReferenceArray iIntersections = new ReferenceArray();
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
                                    if (Math.Round(normal.Z) == 1 || Math.Round(normal.Z) == -1)
                                        iIntersectionFaces.Add(face);
                            }
                            catch { }
                        }
                    }
                }
                //intersections.Add(iIntersections);
                intersectionFaces.Add(iIntersectionFaces);
            }
            return intersectionFaces;
        }

        /// <summary>
        /// Create spot elevations on a faces through a given lines.
        /// </summary>
        /// <returns>Count of created elevation spots.</returns>
        private static int CreateSpots(Document doc, List<Line> lines, List<List<Face>> intersectionFaces)
        {
            int count = 0;

            View view = doc.ActiveView;
            XYZ zero = new XYZ(0, 0, 0);

            // Iterate dimension lines
            for (int i = 0; i < lines.Count; i++)
            {
                // Iterate faces
                for (int j = 0; j < intersectionFaces[i].Count; j++)
                {
                    // Get the center of the face
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
                        count++;
                    }
                    catch { }
                }
            }
            return count;
        }

        /// <summary>
        /// Create dimension on a faces through a given lines.
        /// </summary>
        /// <returns>Count of created dimensions.</returns>
        private static int CreateDimensions(Document doc, List<Line> lines, List<List<Face>> intersectionFaces)
        {
            int count = 0;

            View view = doc.ActiveView;

            for (int i = 0; i < lines.Count; i++)
            {
                // Convert faces to ReferenceArrays
                ReferenceArray references = new ReferenceArray();

                foreach (Face face in intersectionFaces[i])
                {
                    references.Append(face.Reference);
                }

                doc.Create.NewDimension(view, lines[i], references);
                count++;
            }
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
        }
    }
}
