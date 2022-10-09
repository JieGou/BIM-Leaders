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
    [Transaction(TransactionMode.Manual)]
    public class DimensionStairsLandings : IExternalCommand
    {
        private static Document _doc;
        private static DimensionStairsLandingsVM _inputData;
        private static int _countSpots;
        private static int _countDimensions;

        private const string TRANSACTION_NAME = "Annotate Landings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                CheckViewDepth();

                _inputData = GetUserInput();
                if (_inputData == null)
                    return Result.Cancelled;

                List<List<StairsLanding>> landings = GetLandings();
                List<Line> lines = CalculateLines(landings);
                List<List<Face>> intersectionFaces = GetIntersections(landings);
                
                // Create annotations
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    CreateDimensions(lines, intersectionFaces);
                    CreateSpots(lines, intersectionFaces);

                    trans.Commit();
                }

                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static void CheckViewDepth()
        {
            double allowableViewDepth = 1;

            View view = _doc.ActiveView;
            double viewDepth = view.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).AsDouble();

            if (viewDepth > allowableViewDepth)
                TaskDialog.Show("Dimension Stairs", "View depth is too high. This may cause errors. Set far clip offset at most 30 cm.", TaskDialogCommonButtons.Ok);
        }

        private static DimensionStairsLandingsVM GetUserInput()
        {
            DimensionStairsLandingsForm form = new DimensionStairsLandingsForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            DimensionStairsLandingsVM data = form.DataContext as DimensionStairsLandingsVM;

#if VERSION2020
            data.ResultDistance = UnitUtils.ConvertToInternalUnits(data.ResultDistance, DisplayUnitType.DUT_CENTIMETERS);
#else
            data.ResultDistance = UnitUtils.ConvertToInternalUnits(data.ResultDistance, UnitTypeId.Centimeters);
#endif
            return data;
        }

        /// <summary>
        /// Get sorted landings in groups by coordinates, each group have landings with same locations but different heights.
        /// </summary>
        /// <returns>List of lists of landings.</returns>
        private static List<List<StairsLanding>> GetLandings()
        {
            List<List<StairsLanding>> landingsSorted = new List<List<StairsLanding>>();

            View view = _doc.ActiveView;

            // Selecting all landings in the view
            List<StairsLanding> landingsUnsorted = new FilteredElementCollector(_doc, view.Id)
                .OfClass(typeof(StairsLanding))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<StairsLanding>()
                .ToList();

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
                    if (distance < _inputData.ResultDistance)
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
        private static List<Line> CalculateLines(List<List<StairsLanding>> landingsSorted)
        {
            List<Line> lines = new List<Line>();

            View view = _doc.ActiveView;

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
        private static List<List<Face>> GetIntersections(List<List<StairsLanding>> landingsSorted)
        {
            List<List<Face>> intersectionFaces = new List<List<Face>>();

            // Get View
            View view = _doc.ActiveView;

            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = true,
                View = view
            };

            // Iterate through landings solids and get references and faces
            for (int i = 0; i < landingsSorted.Count; i++)
            {
                List<Face> iIntersectionFaces = new List<Face>();

                foreach (StairsLanding landing in landingsSorted[i])
                {
                    // Get solids and sort them from top to bottom.
                    List<Solid> solids = landing.get_Geometry(options)
                        .Where(x => x.GetType() == typeof(Solid))
                        .Cast<Solid>()
                        .OrderBy(x => x.GetBoundingBox().Max.Z)
                        .ToList();

                    if (_inputData.ResultPlacementDimensionTop || _inputData.ResultPlacementElevationTop)
                    {
                        foreach (Face face in solids.First().Faces)
                        {
                            // Some faces are curved so pass them
                            try
                            {
                                // Check if faces are horisontal
                                UV p = new UV(0, 0);
                                XYZ normal = face.ComputeNormal(p);
                                if (Math.Round(normal.X) == 0 && Math.Round(normal.Y) == 0 && Math.Round(normal.Z) == 1)
                                    iIntersectionFaces.Add(face);
                            }
                            catch { }
                        }
                    }

                    if (_inputData.ResultPlacementDimensionMid || _inputData.ResultPlacementElevationMid)
                    {
                        List<Solid> solidsMid = solids.GetRange(1, solids.Count - 1);

                        foreach (Solid solid in solidsMid)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                // Some faces are curved so pass them
                                try
                                {
                                    // Check if faces are horisontal
                                    UV p = new UV(0, 0);
                                    XYZ normal = face.ComputeNormal(p);
                                    if (Math.Round(normal.X) == 0 && Math.Round(normal.Y) == 0 && Math.Round(normal.Z) == 1)
                                        iIntersectionFaces.Add(face);
                                }
                                catch { }
                            }
                        }
                    }

                    if (_inputData.ResultPlacementDimensionBot || _inputData.ResultPlacementElevationBot)
                    {
                        foreach (Face face in solids.Last().Faces)
                        {
                            // Some faces are curved so pass them
                            try
                            {
                                // Check if faces are horisontal
                                UV p = new UV(0, 0);
                                XYZ normal = face.ComputeNormal(p);
                                if (Math.Round(normal.X) == 0 && Math.Round(normal.Y) == 0 && Math.Round(normal.Z) == -1)
                                    iIntersectionFaces.Add(face);
                            }
                            catch { }
                        }
                    }
                }

                intersectionFaces.Add(iIntersectionFaces);
            }
            return intersectionFaces;
        }

        /// <summary>
        /// Create spot elevations on a faces through a given lines.
        /// </summary>
        private static void CreateSpots(List<Line> lines, List<List<Face>> intersectionFaces)
        {
            if (!_inputData.ResultPlacementElevationTop &&
                !_inputData.ResultPlacementElevationMid &&
                !_inputData.ResultPlacementElevationBot)
                return;

            View view = _doc.ActiveView;
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
                        SpotDimension sd = _doc.Create.NewSpotElevation(view, intersectionFaces[i][j].Reference, origin, zero, zero, origin, false);
                        _countSpots++;
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Create dimension on a faces through a given lines.
        /// </summary>
        private static void CreateDimensions(List<Line> lines, List<List<Face>> intersectionFaces)
        {
            if (!_inputData.ResultPlacementDimensionTop &&
                !_inputData.ResultPlacementDimensionMid &&
                !_inputData.ResultPlacementDimensionBot)
                return;

            View view = _doc.ActiveView;

            for (int i = 0; i < lines.Count; i++)
            {
                // Convert faces to ReferenceArrays
                ReferenceArray references = new ReferenceArray();

                foreach (Face face in intersectionFaces[i])
                {
                    if (face.Reference.ElementId != ElementId.InvalidElementId)
                        references.Append(face.Reference);
                }

                Dimension dimension = _doc.Create.NewDimension(view, lines[i], references);

                DimensionUtils.AdjustText(dimension);
                
#if !VERSION2020
                dimension.HasLeader = false;
#endif
                
                _countDimensions++;
            }
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countSpots == 0 && _countDimensions == 0)
                ? "No annotations created."
                : $"{_countSpots} spot elevations were created. {_countDimensions} dimension lines were created.";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
        }
    }
}
