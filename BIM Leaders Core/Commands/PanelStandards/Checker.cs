using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Checker : IExternalCommand
    {
        /// <summary>
        /// Check name for containing given string.
        /// <para>
        /// <c>int count = CountPrefixes(doc, "PRE_", typeof(GutterType));</c>
        /// </para>
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <param name="prefix">String to search.</param>
        /// <param name="type">Sytem.Type of needed DB class (category).</param>
        /// <returns>Count of prefixes that contains given string.</returns>
        private static int CountPrefixes(Document doc, string prefix, Type type)
        {
            int count = 0;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<Element> elements = collector.OfClass(type).ToElements();
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (!name.StartsWith(prefix))
                    count++;
            }

            return count;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Options
            Options options = new Options
            {
                ComputeReferences = false,
                View = doc.ActiveView,
                IncludeNonVisibleObjects = true
            };

            try
            {
                // Collector for data provided in window
                CheckerData data = new CheckerData();

                CheckerForm form = new CheckerForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as CheckerData;

                // Getting input data from user
                string inputPrefix = data.ResultPrefix;
                List<bool> inputCategories = data.ResultCategories;
                List<bool> inputModel = data.ResultModel;
                List<bool> inputCodes = data.ResultCodes;
                int inputHeadHeight = data.ResultHeight;


                int countPrefixes = 0;
                int countGroups = 0;
                int countGroupsUnused = 0;
                int countGroupsUnpinned = 0;
                int countGroupsExcluded = 0;
                int countLinestyles = 0;
                int countRoomsPlacement = 0;
                int countRoomsIntersect = 0;
                int countWarnings = 0;
                int countWallsInterior = 0;
                int countStairsFormula = 0;
                int countHeightLandings = 0;
                int countHeightRuns = 0;


                using (Transaction trans = new Transaction(doc, "Check"))
                {
                    trans.Start();

                    if (inputCategories[0])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(AreaScheme));
                    if (inputCategories[1])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(BrowserOrganization));
                    if (inputCategories[2])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(BuildingPadType));
                    if (inputCategories[3])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(CeilingType));
                    if (inputCategories[4])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(CurtainSystemType));
                    if (inputCategories[5])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(DimensionType));
                    if (inputCategories[6])
                    {
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(Family));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(FamilySymbol));
                    }
                    if (inputCategories[7])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(FilledRegionType));
                    if (inputCategories[8])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(GridType));
                    if (inputCategories[9])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(GroupType));
                    if (inputCategories[10])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(LevelType));
                    if (inputCategories[11])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(LinePatternElement));
                    if (inputCategories[12])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(Material));
                    if (inputCategories[13])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(PanelType));
                    if (inputCategories[14])
                    {
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(ContinuousRailType));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(RailingType));
                    }
                    if (inputCategories[15])
                    {
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(FasciaType));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(GutterType));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(RoofType));
                    }
                    if (inputCategories[16])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(SpotDimensionType));
                    if (inputCategories[17])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(StairsType));
                    if (inputCategories[18])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(StairsLandingType));
                    if (inputCategories[19])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(StairsRunType));
                    if (inputCategories[20])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(TextNoteType));
                    if (inputCategories[21])
                    {
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(ViewDrafting));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(ViewPlan));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(ViewSchedule));
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(ViewSection));
                    }
                    if (inputCategories[22])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(WallType));
                    if (inputCategories[23])
                        countPrefixes += CountPrefixes(doc, inputPrefix, typeof(WallFoundationType));


                    // Groups check
                    if (inputModel[0])
                    {
                        FilteredElementCollector collectorGroupTypes = new FilteredElementCollector(doc);
                        IEnumerable<GroupType> groupTypes = collectorGroupTypes.OfClass(typeof(GroupType))
                            .ToElements().Cast<GroupType>();
                        FilteredElementCollector collectorGroups = new FilteredElementCollector(doc);
                        IEnumerable<Group> groups = collectorGroups.OfClass(typeof(Group))
                            .ToElements().Cast<Group>();

                        // Check size of GroupSet which given from GroupType
                        foreach (GroupType groupType in groupTypes)
                            if (groupType.Groups.Size == 0)
                                countGroupsUnused++;
                        foreach (Group group in groups)
                        {
                            // Check for unpinned groups
                            if (!group.Pinned)
                                countGroupsUnpinned++;

                            // Check if group has excluded elements
                            if (group.Name.EndsWith("(members excluded)"))
                                countGroupsExcluded++;

                            countGroups++;
                        }
                    }


                    // Line Styles Unused check
                    if (inputModel[1])
                    {
                        FilteredElementCollector collectorLines = new FilteredElementCollector(doc);
                        IEnumerable<Line> lines = collectorLines.OfClass(typeof(Line)).WhereElementIsNotElementType().ToElements().Cast<Line>();
                        List<ElementId> lineStylesUsed = new List<ElementId>();
                        foreach (Line line in lines)
                        {
                            ElementId lineStyle = line.GraphicsStyleId;
                            if (!lineStylesUsed.Contains(lineStyle))
                                lineStylesUsed.Add(lineStyle);
                        }
                        // Selecting all line styles in the project
                        CategoryNameMap lineStylesAll = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
                        List<ElementId> lineStyles = new List<ElementId>();
                        foreach (Category category in lineStylesAll)
                            lineStyles.Add(category.Id);
                        foreach (ElementId lineStyle in lineStyles)
                            if (!lineStylesUsed.Contains(lineStyle))
                                countLinestyles++;
                    }


                    // Rooms check
                    if (inputModel[2])
                    {
                        RoomFilter filter = new RoomFilter();
                        FilteredElementCollector collectorRooms = new FilteredElementCollector(doc);
                        IEnumerable<Room> rooms = collectorRooms.OfClass(typeof(Room)).WherePasses(filter).ToElements().Cast<Room>();

                        List<Solid> solids = new List<Solid>();
                        foreach (Room room in rooms)
                        {
                            if (room.Location is null)
                                countRoomsPlacement++;
                            // Checking for volumes overlap
                            else
                            {
                                foreach (Solid solid1 in room.get_Geometry(options))
                                {
                                    foreach (Solid solid2 in solids)
                                    {
                                        Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
                                        if (solid.Volume > 0)
                                            countRoomsIntersect++;
                                    }
                                    solids.Add(solid1);
                                }
                            }
                        }
                    }


                    // Warnings check
                    if (inputModel[3])
                    {
                        IList<FailureMessage> warnings = doc.GetWarnings();
                        countWarnings = warnings.Count();
                    }


                    // Exterior walls check
                    if (inputModel[4])
                    {
                        // Get 3D view with founded first view type, becuase some operations must have 3D in input...
                        FilteredElementCollector collectorViewFamilyTypes = new FilteredElementCollector(doc);
                        IEnumerable<ViewFamilyType> viewFamilyTypes = collectorViewFamilyTypes.OfClass(typeof(ViewFamilyType)).ToElements().Cast<ViewFamilyType>();
                        ElementId viewFamilyTypeId = viewFamilyTypes.First().Id;
                        foreach (ViewFamilyType viewFamilyType in viewFamilyTypes)
                        {
                            if (viewFamilyType.ViewFamily == ViewFamily.ThreeDimensional)
                            {
                                viewFamilyTypeId = viewFamilyType.Id;
                                break;
                            }
                        }
                        View3D viewNew = View3D.CreateIsometric(doc, viewFamilyTypeId);

                        BoundingBoxXYZ box = viewNew.GetSectionBox(); // CHECK !!!

                        // Get perimeter points and lines
                        XYZ point1 = box.Min;
                        XYZ point2 = new XYZ(box.Min.X, box.Max.Y, box.Min.Z);
                        XYZ point3 = new XYZ(box.Max.X, box.Max.Y, box.Min.Z);
                        XYZ point4 = new XYZ(box.Max.X, box.Min.Y, box.Min.Z);

                        CurveLoop curve = new CurveLoop();
                        curve.Append(Line.CreateBound(point1, point2));
                        curve.Append(Line.CreateBound(point2, point3));
                        curve.Append(Line.CreateBound(point3, point4));
                        curve.Append(Line.CreateBound(point4, point1));

                        // Get the lowest level
                        FilteredElementCollector collectorLevels = new FilteredElementCollector(doc);
                        IEnumerable<Level> levels = collectorLevels.OfClass(typeof(Level)).ToElements().Cast<Level>();
                        Level level = levels.First();
                        double elevation = levels.First().Elevation;
                        foreach (Level l in levels)
                            if (l.Elevation < elevation)
                            {
                                level = l;
                                elevation = l.Elevation;
                            }
                        ElementId levelId = level.Id;

                        // Create perimeter walls
                        List<Wall> wallsToDelete = new List<Wall>();
                        foreach (Line line in curve)
                        {
                            Wall wall = Wall.Create(doc, line, levelId, false);
                            Parameter parameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                            parameter.Set(1000);
                            wallsToDelete.Add(wall); // For deleting
                        }

                        // Create room
                        UV point = new UV(box.Min.X + 10, box.Min.Y + 10);
                        Room room = doc.Create.NewRoom(level, point);

                        // Get outer walls
                        List<Element> walls = new List<Element>();
                        SpatialElementBoundaryOptions sebOptions = new SpatialElementBoundaryOptions();
                        IList<IList<BoundarySegment>> segmentsListList = room.GetBoundarySegments(sebOptions);
                        foreach (IList<BoundarySegment> segmentsList in segmentsListList)
                            foreach (BoundarySegment segment in segmentsList)
                            {
                                Element element = doc.GetElement(segment.ElementId);
                                try
                                {
                                    if (element.Category.Name == "Walls")
                                        walls.Add(element);
                                }
                                catch { }
                            }

                        // Check if walls are Exterior
                        foreach (Wall wall in walls)
                            if (wall.WallType.Function == WallFunction.Interior)
                                countWallsInterior++;

                        // Remove created 4 walls if needed
                        if (wallsToDelete[0].WallType.Function == WallFunction.Interior)
                            countWallsInterior -= 4;

                        // Deleting
                        doc.Delete(room.Id);
                        foreach (Wall wall in wallsToDelete)
                            doc.Delete(wall.Id);
                    }


                    // Checking stairs formula
                    if (inputCodes[0])
                    {
                        FilteredElementCollector collectorStairs = new FilteredElementCollector(doc);
                        IEnumerable<Stairs> stairs = collectorStairs.OfClass(typeof(Stairs)).ToElements().Cast<Stairs>();

                        // Check if stairs steps are right height and depth
                        foreach (Stairs stair in stairs)
                        {
#if VERSION2020
                            double stepHeight = UnitUtils.ConvertFromInternalUnits(stair.ActualRiserHeight, DisplayUnitType.DUT_CENTIMETERS);
                            double stepDepth = UnitUtils.ConvertFromInternalUnits(stair.ActualTreadDepth, DisplayUnitType.DUT_CENTIMETERS);
#else
                            double stepHeight = UnitUtils.ConvertFromInternalUnits(stair.ActualRiserHeight, UnitTypeId.Centimeters);
                            double stepDepth = UnitUtils.ConvertFromInternalUnits(stair.ActualTreadDepth, UnitTypeId.Centimeters);
#endif
                            double r = 2 * stepHeight + stepDepth;

                            if (r < 61 | r > 63 || stepHeight < 10 || stepHeight > 17.5 || stepDepth < 26)
                                countStairsFormula++;
                        }
                    }


                    // Checking stairs head height
                    if (inputCodes[1])
                    {
#if VERSION2020
                        double heightOffset = UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_CENTIMETERS);
                        double height = UnitUtils.ConvertToInternalUnits(inputHeadHeight, DisplayUnitType.DUT_CENTIMETERS) - heightOffset;
                        double planOffset = UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_CENTIMETERS);
#else
                        double heightOffset = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Centimeters);
                        double height = UnitUtils.ConvertToInternalUnits(inputHeadHeight, UnitTypeId.Centimeters) - heightOffset;
                        double planOffset = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);
#endif
                        // Get extrusion vector, only direction is needed but for sure the point is on 0 point of solid bottom but Z is on solid top
                        XYZ extrusionDir = new XYZ(0, 0, 1);

                        FilteredElementCollector collectorStairs = new FilteredElementCollector(doc);
                        IEnumerable<Stairs> stairs = collectorStairs.OfClass(typeof(Stairs)).ToElements().Cast<Stairs>();

                        // Check
                        foreach (Stairs stair in stairs)
                        {
                            // Get landings
                            ICollection<ElementId> landingIds = stair.GetStairsLandings();
                            List<StairsLanding> landings = new List<StairsLanding>();
                            foreach (ElementId landingId in landingIds)
                                landings.Add(doc.GetElement(landingId) as StairsLanding);

                            // Check landings geometry
                            foreach (StairsLanding landing in landings)
                            {
                                double landingElevation = landing.BaseElevation;
                                CurveLoop landingSketch = landing.GetFootprintBoundary();
                                // Offset the curve loop for not intersect walls
                                CurveLoop landingSketchOffset = CurveLoop.CreateViaOffset(landingSketch, -planOffset, new XYZ(0, 0, 1));
                                List<CurveLoop> landingSketchOffsetList = new List<CurveLoop> { landingSketchOffset };

                                // Create solid, with solid height of "height_offset"
                                Solid landingSolid = GeometryCreationUtilities.CreateExtrusionGeometry(landingSketchOffsetList, extrusionDir, heightOffset);
                                // Transform solid to the needed height
                                Transform transform = Transform.CreateTranslation(new XYZ(0, 0, height + landingElevation));
                                Solid landingSolidTransformed = SolidUtils.CreateTransformed(landingSolid, transform);

                                // Create filter to get intersecting elements
                                ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(landingSolidTransformed);
                                FilteredElementCollector collectorIntersect = new FilteredElementCollector(doc);
                                IList<Element> elementsIntersects = collectorIntersect.WherePasses(filter).ToElements();

                                countHeightLandings += elementsIntersects.Count;
                            }
                            
                            // Get runs
                            ICollection<ElementId> runIds = stair.GetStairsRuns();
                            List<StairsRun> runs = new List<StairsRun>();
                            foreach (ElementId runId in runIds)
                                runs.Add(doc.GetElement(runId) as StairsRun);

                            // Check runs geometry
                            foreach (StairsRun run in runs)
                            {
                                double countHeightSteps = 0;

                                GeometryElement runGeometry = run.get_Geometry(options);

                                List<Solid> solids = new List<Solid>();
                                List<double> heights = new List<double>();

                                foreach (Solid solid in runGeometry.OfType<Solid>())
                                {
                                    if (solid.Volume > 0)
                                    {
                                        solids.Add(solid);
                                        heights.Add(solid.GetBoundingBox().Max.Z);
                                    }
                                }

                                FaceArray runFacesAll = solids[heights.IndexOf(heights.Max())].Faces;

                                // Filter faces by normal going up
                                List<Face> runFaces = new List<Face>();
                                foreach (PlanarFace runFace in runFacesAll)
                                {
                                    double runFaceNormalX = runFace.FaceNormal.X;
                                    double runFaceNormalY = runFace.FaceNormal.Y;
                                    double runFaceNormalZ = runFace.FaceNormal.Z;
                                    if (runFaceNormalX == 0 && runFaceNormalY == 0 && runFaceNormalY == 1)
                                        runFaces.Add(runFace);
                                }

                                // Making solid and check
                                foreach (PlanarFace runFace in runFaces)
                                {
                                    IList<CurveLoop> runFaceEdgesList = runFace.GetEdgesAsCurveLoops();
                                    // Offset for not to touch walls
                                    List<CurveLoop> runFaceEdgesOffsetList = new List<CurveLoop>();
                                    foreach (CurveLoop runFaceEdges in runFaceEdgesList)
                                    {
                                        CurveLoop runFaceEdgesOffset = CurveLoop.CreateViaOffset(runFaceEdgesList[0], -planOffset, runFace.FaceNormal);
                                        runFaceEdgesOffsetList.Add(runFaceEdgesOffset);
                                    }
                                    
                                    // Create solid on "height" height from landing, with solid height of "height_offset"
                                    Solid solidRun = GeometryCreationUtilities.CreateExtrusionGeometry(runFaceEdgesOffsetList, extrusionDir, heightOffset);
                                    // Transform solid to the needed height

                                    Transform transform = Transform.CreateTranslation(new XYZ(0, 0, height));
                                    Solid solidRunTransformed = SolidUtils.CreateTransformed(solidRun, transform);

                                    // Create filter to get intersecting elements
                                    ElementIntersectsSolidFilter faceFilter = new ElementIntersectsSolidFilter(solidRunTransformed);
                                    FilteredElementCollector collectorIntersect = new FilteredElementCollector(doc);
                                    IList<Element> er = collectorIntersect.WherePasses(faceFilter).ToElements();

                                    if (er.Count > 0)
                                        countHeightSteps++;
                                }

                                if (countHeightSteps > 0)
                                    countHeightRuns++;
                            }
                        }

                        trans.Commit();


                        // Export to Excel
                        // ...

                        // Create a DataSet
                        DataSet reportDataSet = new DataSet("reportDataSet");
                        // Create DataTable
                        DataTable reportDataTable = new DataTable("Report");
                        // Create 4 columns, and add them to the table
                        DataColumn reportColumnCheck = new DataColumn("Check", typeof(string));
                        DataColumn reportColumnResult = new DataColumn("Result", typeof(string));

                        reportDataTable.Columns.Add(reportColumnCheck);
                        reportDataTable.Columns.Add(reportColumnResult);

                        // Add the table to the DataSet
                        reportDataSet.Tables.Add(reportDataTable);

                        // Fill the table

                        // Prefixes
                        string iCheck = "Prefixes";
                        string iResult = "-";

                        if (countPrefixes != 0)
                            iResult = string.Format("{0} prefixes wrong.", countPrefixes.ToString());
                        DataRow newRow1 = reportDataTable.NewRow();
                        newRow1["Check"] = iCheck;
                        newRow1["Result"] = iResult;

                        // Groups Unused
                        iCheck = "Unused Groups";
                        iResult = "-";
                        if (countGroupsUnused != 0)
                            iResult = string.Format("{0} of {1} groups are not used.", countGroupsUnused.ToString(), countGroups.ToString());
                        DataRow newRow2 = reportDataTable.NewRow();
                        newRow2["Check"] = iCheck;
                        newRow2["Result"] = iResult;

                        // Groups Unpined
                        iCheck = "Unpinned Groups";
                        iResult = "-";
                        if (countGroupsUnpinned != 0)
                            iResult = string.Format("{0} of {1} groups are not pinned.", countGroupsUnpinned.ToString(), countGroups.ToString());
                        DataRow newRow3 = reportDataTable.NewRow();
                        newRow3["Check"] = iCheck;
                        newRow3["Result"] = iResult;

                        // Groups Excluded
                        iCheck = "Excluded Groups";
                        iResult = "-";
                        if (countGroupsExcluded != 0)
                            iResult = string.Format("{0} of {1} group instances are with excluded elements.", countGroupsExcluded.ToString(), countGroups.ToString());
                        DataRow newRow4 = reportDataTable.NewRow();
                        newRow4["Check"] = iCheck;
                        newRow4["Result"] = iResult;

                        // Linestyles
                        iCheck = "Line Styles";
                        iResult = "-";
                        if (countLinestyles != 0)
                            iResult = string.Format("{0} line styles are unused.", countLinestyles.ToString());
                        DataRow newRow5 = reportDataTable.NewRow();
                        newRow5["Check"] = iCheck;
                        newRow5["Result"] = iResult;

                        // Rooms Placed
                        iCheck = "Rooms Placement";
                        iResult = "-";
                        if (countRoomsPlacement != 0)
                            iResult = string.Format("{0} rooms are not placed.", countRoomsPlacement.ToString());
                        DataRow newRow6 = reportDataTable.NewRow();
                        newRow6["Check"] = iCheck;
                        newRow6["Result"] = iResult;

                        // Rooms Overlap
                        iCheck = "Rooms Overlap";
                        iResult = "-";
                        if (countRoomsIntersect != 0)
                            iResult = string.Format("{0} rooms overlap.", countRoomsIntersect.ToString());
                        DataRow newRow7 = reportDataTable.NewRow();
                        newRow7["Check"] = iCheck;
                        newRow7["Result"] = iResult;

                        // Warnings
                        iCheck = "Warnings";
                        iResult = "-";
                        if (countWarnings != 0)
                            iResult = string.Format("{0} warnings in the project.", countWarnings.ToString());
                        DataRow newRow8 = reportDataTable.NewRow();
                        newRow8["Check"] = iCheck;
                        newRow8["Result"] = iResult;

                        // Walls Interior
                        iCheck = "Walls";
                        iResult = "-";
                        if (countWallsInterior != 0)
                            iResult = string.Format("{0} exterior walls have interior type.", countWallsInterior.ToString());
                        DataRow newRow9 = reportDataTable.NewRow();
                        newRow9["Check"] = iCheck;
                        newRow9["Result"] = iResult;

                        // Stairs Formula
                        iCheck = "Stairs Formula";
                        iResult = "-";
                        if (countStairsFormula != 0)
                            iResult = string.Format("{0} stairs have bad formula.", countStairsFormula.ToString());
                        DataRow newRow10 = reportDataTable.NewRow();
                        newRow10["Check"] = iCheck;
                        newRow10["Result"] = iResult;

                        // Stairs Landings
                        iCheck = "Stairs Head Height - Landings";
                        iResult = "-";
                        if (countHeightLandings != 0)
                            iResult = string.Format("{0} stairs landings have too low head height.", countHeightLandings.ToString());
                        DataRow newRow11 = reportDataTable.NewRow();
                        newRow11["Check"] = iCheck;
                        newRow11["Result"] = iResult;

                        // Stairs Runs
                        iCheck = "Stairs Head Height - Runs";
                        iResult = "-";
                        if (countHeightRuns != 0)
                            iResult = string.Format("{0} stairs runs have too low head height.", countHeightRuns.ToString());
                        DataRow newRow12 = reportDataTable.NewRow();
                        newRow12["Check"] = iCheck;
                        newRow12["Result"] = iResult;

                        // Add the rows to the Report table
                        reportDataTable.Rows.Add(newRow1);
                        reportDataTable.Rows.Add(newRow2);
                        reportDataTable.Rows.Add(newRow3);
                        reportDataTable.Rows.Add(newRow4);
                        reportDataTable.Rows.Add(newRow5);
                        reportDataTable.Rows.Add(newRow6);
                        reportDataTable.Rows.Add(newRow7);
                        reportDataTable.Rows.Add(newRow8);
                        reportDataTable.Rows.Add(newRow9);
                        reportDataTable.Rows.Add(newRow10);
                        reportDataTable.Rows.Add(newRow11);
                        reportDataTable.Rows.Add(newRow12);

                        // Show result
                        CheckerReportData dataReport = new CheckerReportData(reportDataSet);
                        CheckerReportForm formReport = new CheckerReportForm(reportDataSet);

                        formReport.ShowDialog();

                        if (formReport.DialogResult == false)
                            return Result.Cancelled;
                    }
                    return Result.Succeeded;
                }
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
            return typeof(Checker).Namespace + "." + nameof(Checker);
        }
    }
}
