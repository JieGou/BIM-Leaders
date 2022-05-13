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

                List<ReportMessage> reportMessageList = new List<ReportMessage>();

                using (Transaction trans = new Transaction(doc, "Check"))
                {
                    trans.Start();

                    reportMessageList.AddRange(CheckPrefixesAll(doc, inputCategories, inputPrefix));

                    // Check for empty tags.
                    if (inputModel[0])
                        reportMessageList.AddRange(CheckTags(doc));

                    // Check for count of text notes in the project.
                    if (inputModel[1])
                        reportMessageList.AddRange(CheckTextNotes(doc));

                    // Check if filters are unused.
                    if (inputModel[2])
                        reportMessageList.AddRange(CheckFilters(doc));

                    // Check for sheet placeholders and empty sheets.
                    if (inputModel[6])
                        reportMessageList.AddRange(CheckSheets(doc));

                    // Groups check
                    if (inputModel[8])
                        reportMessageList.AddRange(CheckGroups(doc));

                    // Line Styles Unused check
                    if (inputModel[2])
                        reportMessageList.AddRange(CheckLineStyles(doc));

                    // Rooms check
                    if (inputModel[9])
                        reportMessageList.AddRange(CheckRooms(doc));

                    // Warnings check
                    if (inputModel[7])
                        reportMessageList.AddRange(CheckWarnings(doc));

                    // Exterior walls check
                    if (inputModel[13])
                        reportMessageList.AddRange(CheckWallsExterior(doc));

                    // Checking stairs formula
                    if (inputCodes[0])
                        reportMessageList.AddRange(CheckStairsFormula(doc));

                    // Checking stairs head height
                    if (inputCodes[1])
                        reportMessageList.AddRange(CheckStairsHeadHeight(doc, inputHeadHeight));
                    
                    trans.Commit();
                }

                // Export to Excel
                // ...

                // Create a DataSet
                DataSet reportDataSet = CreateReportDataSet(reportMessageList);

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
        /// Message class containing message name and text.
        /// </summary>
        private class ReportMessage
        {
            public string MessageName { get; set; }
            public string MessageText { get; set; }
            public ReportMessage(string messageName, string messageText)
            {
                MessageName = messageName;
                MessageText = messageText;
            }
        }

        /// <summary>
        /// Check names for containing given string.
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckPrefixesAll(Document doc, List<bool> inputCategories, string prefix)
        {
            int countPrefixes = 0;

            List<Type> types = Categories.GetTypesList(inputCategories);

            foreach (Type type in types)
                countPrefixes += CheckPrefixes(doc, prefix, type);

            string reportMessageText = (countPrefixes == 0)
                ? "-"
                : $"{countPrefixes} prefixes wrong.";
            ReportMessage reportMessage = new ReportMessage("Prefixes", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
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
        /// Checks if any empty tags in the model.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckTags(Document doc)
        {
            int countTags = 0;

            IEnumerable<IndependentTag> tags = new FilteredElementCollector(doc)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>()
                .Where(x => x.TagText.Length == 0);

            countTags = tags.Count();

            string reportMessageText = (countTags == 0)
                ? "-"
                : $"{countTags} tags are empty.";
            ReportMessage reportMessage = new ReportMessage("Empty Tags", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Check for count of text notes in the project.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckTextNotes(Document doc)
        {
            IEnumerable<TextNote> textNotes = new FilteredElementCollector(doc)
                .OfClass(typeof(TextNote))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<TextNote>();

            int countTextNotes = textNotes.Count();
            string reportMessageText = (countTextNotes == 0)
                ? "-"
                : $"{countTextNotes} text notes are in the project.";
            ReportMessage reportMessage = new ReportMessage("Text Notes", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Check if filters are unused.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckFilters(Document doc)
        {
            int countFiltersUnused = 0;

            IEnumerable<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();
            ICollection<ElementId> filtersAll = new FilteredElementCollector(doc)
                .OfClass(typeof(ElementFilter))
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get views filters list
            List<ElementId> filtersUsed = new List<ElementId>();
            foreach (View view in views)
            {
                // Add to the list if it not contains those filters yet
                ICollection<ElementId> viewFilters = view.GetFilters();
                filtersUsed.AddRange(viewFilters.Where(x => !filtersUsed.Contains(x)));
            }

            countFiltersUnused = filtersAll.Count - filtersUsed.Count;

            string reportMessageText = (countFiltersUnused == 0)
                ? "-"
                : $"{countFiltersUnused} filters are unused.";
            ReportMessage reportMessage = new ReportMessage("Unused Filters", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Check for sheet placeholders and empty sheets.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckSheets(Document doc)
        {
            int countPlaceholders = 0;
            int countEmptySheets = 0;

            IEnumerable<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ViewSheet>()
                .Where(x => x.GetAllPlacedViews().Count == 0);

            List<ViewSheet> sheetsNoPlaceholders = new List<ViewSheet>();
            foreach (ViewSheet sheet in sheets)
            {
                if (sheet.IsPlaceholder)
                    countPlaceholders++;
                else
                    sheetsNoPlaceholders.Add(sheet);
            }

            // "Empty" sheets can contain schedules still, so filter sheets without schedules.
            IEnumerable<ElementId> schedulesSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ScheduleSheetInstance>()
                .Select(x => x.OwnerViewId);

            foreach (ViewSheet sheet in sheetsNoPlaceholders)
            {
                if (!schedulesSheets.Contains(sheet.Id))
                    countEmptySheets++;
            }

            string reportMessageText0 = (countPlaceholders == 0)
                ? "-"
                : $"{countPlaceholders} placeholder sheets are in the project.";
            ReportMessage reportMessage0 = new ReportMessage("Placeholder Sheets", reportMessageText0);

            string reportMessageText1 = (countEmptySheets == 0)
                ? "-"
                : $"{countEmptySheets} empty sheets are in the project.";
            ReportMessage reportMessage1 = new ReportMessage("Empty Sheets", reportMessageText1);

            return new List<ReportMessage>() { reportMessage0, reportMessage1 };
        }

        /// <summary>
        /// Check groups in current document for unused, unpinned, excluded groups.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckGroups(Document doc)
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

            string reportMessageText0 = (countGroupsUnused == 0)
                ? "-"
                : $"{countGroupsUnused} of {countGroups} groups are not used.";
            ReportMessage reportMessage0 = new ReportMessage("Unused Groups", reportMessageText0);

            string reportMessageText1 = (countGroupsUnpinned == 0)
                ? "-"
                : $"{countGroupsUnpinned} of {countGroups} groups are not pinned.";
            ReportMessage reportMessage1 = new ReportMessage("Unpinned Groups", reportMessageText1);

            string reportMessageText2 = (countGroupsExcluded == 0)
                ? "-"
                : $"{countGroupsExcluded} of {countGroups} group instances are with excluded elements.";
            ReportMessage reportMessage2 = new ReportMessage("Excluded Groups", reportMessageText2);

            return new List<ReportMessage>() { reportMessage0, reportMessage1, reportMessage2 };
        }

        /// <summary>
        /// Checks if all existing linestyles are used in the document.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckLineStyles(Document doc)
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

            string reportMessageText = (countLinestyles == 0)
                ? "-"
                : $"{countLinestyles} line styles are unused.";
            ReportMessage reportMessage = new ReportMessage("Line Styles", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Checks all rooms in the document.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckRooms(Document doc)
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

            string reportMessageText0 = (countRoomsPlacement == 0)
                ? "-"
                : $"{countRoomsPlacement} rooms are not placed.";
            ReportMessage reportMessage0 = new ReportMessage("Rooms Placement", reportMessageText0);

            string reportMessageText1 = (countRoomsIntersect == 0)
                ? "-"
                : $"{countRoomsIntersect} rooms overlap.";
            ReportMessage reportMessage1 = new ReportMessage("Rooms Overlap", reportMessageText1);

            return new List<ReportMessage>() { reportMessage0, reportMessage1 };
        }

        /// <summary>
        /// Checks if warnings are in the document.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckWarnings(Document doc)
        {
            int countWarnings = doc.GetWarnings().Count;

            string reportMessageText = (countWarnings == 0)
                ? "-"
                : $"{countWarnings} warnings in the project.";
            ReportMessage reportMessage = new ReportMessage("Warnings", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Checks if all actually exterior walls are set as Exterior in the properties.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckWallsExterior(Document doc)
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

            string reportMessageText = (countWallsInterior == 0)
                ? "-"
                : $"{countWallsInterior} exterior walls have interior type.";
            ReportMessage reportMessage = new ReportMessage("Exterior Walls", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Checks all stairs if their parameters (step length / step height) good in formula.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckStairsFormula(Document doc)
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

            string reportMessageText = (countStairsFormula == 0)
                ? "-"
                : $"{countStairsFormula} stairs have bad formula.";
            ReportMessage reportMessage = new ReportMessage("Stairs Formula", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Checks all stairs elements in document if they good in the given head height.
        /// </summary>
        /// <returns>Checking report messages.</returns>
        private static IEnumerable<ReportMessage> CheckStairsHeadHeight(Document doc, double inputHeadHeight)
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

            string reportMessageText = "";
            if (countHeightLandings != 0)
                reportMessageText += $"{countHeightLandings} stairs landings";
            if (countHeightRuns != 0)
                if (reportMessageText.Length != 0)
                    reportMessageText += " and ";
                reportMessageText += $"{countHeightLandings} stairs runs";
            if (reportMessageText.Length != 0)
                reportMessageText += " have too low head height.";
            else
                reportMessageText = "-";

            ReportMessage reportMessage = new ReportMessage("Stairs Head Height", reportMessageText);

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Create DataSet table for report window.
        /// </summary>
        /// <returns>DataSet object contains all data.</returns>
        private static DataSet CreateReportDataSet(List<ReportMessage> reportMessages)
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
            foreach (ReportMessage reportMessage in reportMessages)
            {
                DataRow dataRow = reportDataTable.NewRow();
                dataRow["Check"] = reportMessage.MessageName;
                dataRow["Result"] = reportMessage.MessageText;
                reportDataTable.Rows.Add(dataRow);
            }

            return reportDataSet;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Checker).Namespace + "." + nameof(Checker);
        }
    }
}
