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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

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

                    countPrefixes = CheckPrefixesAll(doc, inputCategories, inputPrefix);

                    // Groups check
                    if (inputModel[0])
                        (countGroups, countGroupsUnused, countGroupsUnpinned, countGroupsExcluded) = CheckGroups(doc);

                    // Line Styles Unused check
                    if (inputModel[1])
                        countLinestyles = CheckLineStyles(doc);

                    // Rooms check
                    if (inputModel[2])
                        (countRoomsPlacement, countRoomsIntersect) = CheckRooms(doc);

                    // Warnings check
                    if (inputModel[3])
                        countWarnings = doc.GetWarnings().Count;

                    // Exterior walls check
                    if (inputModel[4])
                        countWallsInterior = CheckWallsExterior(doc);

                    // Checking stairs formula
                    if (inputCodes[0])
                        countStairsFormula = CheckStairsFormula(doc);

                    // Checking stairs head height
                    if (inputCodes[1])
                        (countHeightLandings, countHeightRuns) = CheckStairsHeadHeight(doc, inputHeadHeight);
                    
                    trans.Commit();
                }

                // Export to Excel
                // ...

                // Create a DataSet
                DataSet reportDataSet = CreateReportDataSet(
                    countPrefixes,
                    countGroups,
                    countGroupsUnused,
                    countGroupsUnpinned,
                    countGroupsExcluded,
                    countLinestyles,
                    countRoomsPlacement,
                    countRoomsIntersect,
                    countWarnings,
                    countWallsInterior,
                    countStairsFormula,
                    countHeightLandings,
                    countHeightRuns);

                // Show result
                //CheckerReportData dataReport = new CheckerReportData(reportDataSet);
                CheckerReportForm formReport = new CheckerReportForm(reportDataSet);

                formReport.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Check names for containing given string.
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <returns>Count of prefixes that contains given string.</returns>
        private static int CheckPrefixesAll(Document doc, List<bool> inputCategories, string prefix)
        {
            int countPrefixes = 0;

            Dictionary<Type, bool> categories = new Dictionary<Type, bool>(){
                { typeof(AreaScheme),          inputCategories[0] },
                { typeof(BrowserOrganization), inputCategories[1] },
                { typeof(BuildingPadType),     inputCategories[2] },
                { typeof(CeilingType),         inputCategories[3] },
                { typeof(CurtainSystemType),   inputCategories[4] },
                { typeof(DimensionType),       inputCategories[5] },
                { typeof(Family),              inputCategories[6] },
                { typeof(FamilySymbol),        inputCategories[6] },
                { typeof(FilledRegionType),    inputCategories[7] },
                { typeof(GridType),            inputCategories[8] },
                { typeof(GroupType),           inputCategories[9] },
                { typeof(LevelType),           inputCategories[10] },
                { typeof(LinePatternElement),  inputCategories[11] },
                { typeof(Material),            inputCategories[12] },
                { typeof(PanelType),           inputCategories[13] },
                { typeof(ContinuousRailType),  inputCategories[14] },
                { typeof(RailingType),         inputCategories[14] },
                { typeof(FasciaType),          inputCategories[15] },
                { typeof(GutterType),          inputCategories[15] },
                { typeof(RoofType),            inputCategories[15] },
                { typeof(SpotDimensionType),   inputCategories[16] },
                { typeof(StairsType),          inputCategories[17] },
                { typeof(StairsLandingType),   inputCategories[18] },
                { typeof(StairsRunType),       inputCategories[19] },
                { typeof(TextNoteType),        inputCategories[20] },
                { typeof(ViewDrafting),        inputCategories[21] },
                { typeof(ViewPlan),            inputCategories[21] },
                { typeof(ViewSchedule),        inputCategories[21] },
                { typeof(ViewSection),         inputCategories[21] },
                { typeof(WallType),            inputCategories[22] },
                { typeof(WallFoundationType),  inputCategories[23] }
            };

            foreach (KeyValuePair<Type, bool> keyValue in categories)
                if (keyValue.Value)
                    countPrefixes += CheckPrefixes(doc, prefix, keyValue.Key);

            return countPrefixes;
        }

        /// <summary>
        /// Check names for containing given string.
        /// <para>
        /// <c>int count = CountPrefixes(doc, "PRE_", typeof(GutterType));</c>
        /// </para>
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <param name="prefix">String to search.</param>
        /// <param name="type">Sytem.Type of needed DB class (category).</param>
        /// <returns>Count of prefixes that contains given string.</returns>
        private static int CheckPrefixes(Document doc, string prefix, Type type)
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

        /// <summary>
        /// Check groups in current document for unused, unpinned, excluded groups.
        /// </summary>
        /// <returns>Tuple of counts of wrong groups.</returns>
        private static (int, int, int, int) CheckGroups(Document doc)
        {
            int countGroups = 0;
            int countGroupsUnused = 0;
            int countGroupsUnpinned = 0;
            int countGroupsExcluded = 0;

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

            return (countGroups, countGroupsUnused, countGroupsUnpinned, countGroupsExcluded);
        }

        /// <summary>
        /// Checks if all existing linestyles are used in the document.
        /// </summary>
        /// <returns>Count of unused linestyles.</returns>
        private static int CheckLineStyles(Document doc)
        {
            int countLinestyles = 0;

            IEnumerable<CurveElement> lines = new FilteredElementCollector(doc)
                .OfClass(typeof(CurveElement))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<CurveElement>();
            List<ElementId> lineStylesUsed = new List<ElementId>();
            foreach (CurveElement line in lines)
            {
                ElementId lineStyle = line.LineStyle.Id;
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

            return countLinestyles;
        }

        /// <summary>
        /// Checks all rooms in the document.
        /// </summary>
        /// <returns>Count of all unplaced and intersecting rooms.</returns>
        private static (int, int) CheckRooms(Document doc)
        {
            int countRoomsPlacement = 0;
            int countRoomsUnbounded = 0;
            int countRoomsIntersect = 0;

            Options options = new Options
            {
                ComputeReferences = false,
                View = doc.ActiveView,
                IncludeNonVisibleObjects = true
            };

            IEnumerable<Room> rooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .WherePasses(new RoomFilter())
                .ToElements()
                .Cast<Room>();

            List<Solid> solidsCollected = new List<Solid>();
            foreach (Room room in rooms)
            {
                if (room.Location is null)
                    countRoomsPlacement++;
                else if (room.Volume == 0)
                    countRoomsUnbounded++; // NOW WITHOUT OUTPUT AND REPORT
                // Checking for volumes overlap
                else
                {
                    foreach (Solid solidRoom in room.get_Geometry(options))
                    {
                        foreach (Solid solidCollected in solidsCollected)
                        {
                            Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation(solidRoom, solidCollected, BooleanOperationsType.Intersect);
                            if (solid.Volume > 0)
                                countRoomsIntersect++;
                        }
                        solidsCollected.Add(solidRoom);
                    }
                }
            }

            return (countRoomsPlacement, countRoomsIntersect);
        }

        /// <summary>
        /// Checks if all actually exterior walls are set as Exterior in the properties.
        /// </summary>
        /// <returns>Count of wrong walls.</returns>
        private static int CheckWallsExterior(Document doc)
        {
            int countWallsInterior = 0;

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

            return countWallsInterior;
        }

        /// <summary>
        /// Checks all stairs if their parameters (step length / step height) good in formula.
        /// </summary>
        /// <returns>Count of wrong stairs.</returns>
        private static int CheckStairsFormula(Document doc)
        {
            int countStairsFormula = 0;

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
            return countStairsFormula;
        }

        /// <summary>
        /// Checks all stairs elements in document if they good in the given head height.
        /// </summary>
        /// <returns>Count of wrong stairs landings and runs.</returns>
        private static (int, int) CheckStairsHeadHeight(Document doc, double inputHeadHeight)
        {
            int countHeightLandings = 0;
            int countHeightRuns = 0;

            Options options = new Options
            {
                ComputeReferences = false,
                View = doc.ActiveView,
                IncludeNonVisibleObjects = true
            };

#if VERSION2020
            double heightOffset = UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_CENTIMETERS);
            double height = UnitUtils.ConvertToInternalUnits(inputHeadHeight, DisplayUnitType.DUT_CENTIMETERS) - heightOffset;
            double planOffset = UnitUtils.ConvertToInternalUnits(1, DisplayUnitType.DUT_CENTIMETERS); // Offset for not touching walls near stairs
#else
            double heightOffset = UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Centimeters);
            double height = UnitUtils.ConvertToInternalUnits(inputHeadHeight, UnitTypeId.Centimeters) - heightOffset;
            double planOffset = UnitUtils.ConvertToInternalUnits(1, UnitTypeId.Centimeters);
#endif
            // Get extrusion vector, only direction is needed but for sure the point is on 0 point of solid bottom but Z is on solid top
            XYZ extrusionDir = new XYZ(0, 0, 1);

            IEnumerable<Stairs> stairs = new FilteredElementCollector(doc)
                .OfClass(typeof(Stairs))
                .ToElements()
                .Cast<Stairs>();

            // Check
            foreach (Stairs stair in stairs)
            {
                // Get landings
                List<StairsLanding> landings = new List<StairsLanding>();
                ICollection<ElementId> landingIds = stair.GetStairsLandings();
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
                    IList<Element> elementsIntersects = new FilteredElementCollector(doc)
                        .WherePasses(new ElementIntersectsSolidFilter(landingSolidTransformed))
                        .ToElements();

                    countHeightLandings += elementsIntersects.Count;
                }

                // Get runs
                List<StairsRun> runs = new List<StairsRun>();
                ICollection<ElementId> runIds = stair.GetStairsRuns();
                foreach (ElementId runId in runIds)
                    runs.Add(doc.GetElement(runId) as StairsRun);

                // Check runs geometry
                foreach (StairsRun run in runs)
                {
                    double countHeightSteps = 0;

                    List<Solid> runSolids = new List<Solid>();
                    List<double> runSolidsMaxHeights = new List<double>();
                    foreach (Solid solid in run.get_Geometry(options).OfType<Solid>())
                    {
                        if (solid.Volume > 0)
                        {
                            runSolids.Add(solid);
                            runSolidsMaxHeights.Add(solid.GetBoundingBox().Max.Z);
                        }
                    }

                    if (runSolids.Count == 0)
                        continue;

                    // Get faces of the highest run solid (finish layer)
                    FaceArray runFacesAll = runSolids[runSolidsMaxHeights.IndexOf(runSolidsMaxHeights.Max())].Faces;

                    // Filter faces by normal going up
                    List<Face> runFaces = new List<Face>();
                    foreach (PlanarFace runFace in runFacesAll)
                        if (runFace.FaceNormal.Z == 1)
                            runFaces.Add(runFace);
                    
                    // Making solids from faces and check height
                    foreach (PlanarFace runFace in runFaces)
                    {
                        IList<CurveLoop> runFaceEdgesList = runFace.GetEdgesAsCurveLoops();
                        // Offset for not to touch walls
                        List<CurveLoop> runFaceEdgesOffsetList = new List<CurveLoop>();
                        foreach (CurveLoop runFaceEdges in runFaceEdgesList)
                        {
                            // Catching Internal Revit Error
                            try
                            {
                                CurveLoop runFaceEdgesOffset = CurveLoop.CreateViaOffset(runFaceEdgesList[0], -planOffset, runFace.FaceNormal);
                                runFaceEdgesOffsetList.Add(runFaceEdgesOffset);
                            }
                            catch { }
                        }

                        if (runFaceEdgesOffsetList.Count == 0)
                            continue;

                        // Create solid on "height" height from landing, with solid height of "height_offset"
                        Solid solidRun = GeometryCreationUtilities.CreateExtrusionGeometry(runFaceEdgesOffsetList, extrusionDir, heightOffset);

                        // Transform solid to the needed height
                        Transform transform = Transform.CreateTranslation(new XYZ(0, 0, height));
                        Solid solidRunTransformed = SolidUtils.CreateTransformed(solidRun, transform);

                        // Create filter to get intersecting elements
                        IList<Element> er = new FilteredElementCollector(doc)
                            .WherePasses(new ElementIntersectsSolidFilter(solidRunTransformed))
                            .ToElements();
                        
                        if (er.Count > 0)
                            countHeightSteps++;
                    }

                    if (countHeightSteps > 0)
                        countHeightRuns++;
                }
            }
            return (countHeightLandings, countHeightRuns);
        }

        /// <summary>
        /// Create DataSet table for report window.
        /// </summary>
        /// <returns>DataSet object contains all data.</returns>
        private static DataSet CreateReportDataSet(
            int countPrefixes,
            int countGroups,
            int countGroupsUnused,
            int countGroupsUnpinned,
            int countGroupsExcluded,
            int countLinestyles,
            int countRoomsPlacement,
            int countRoomsIntersect,
            int countWarnings,
            int countWallsInterior,
            int countStairsFormula,
            int countHeightLandings,
            int countHeightRuns)
        {
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

            return reportDataSet;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Checker).Namespace + "." + nameof(Checker);
        }
    }
}
