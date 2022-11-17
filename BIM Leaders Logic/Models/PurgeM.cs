using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class PurgeM : BaseModel
    {
        #region PROPERTIES

        private bool _purgeRooms;
        public bool PurgeRooms
        {
            get { return _purgeRooms; }
            set
            {
                _purgeRooms = value;
                OnPropertyChanged(nameof(PurgeRooms));
            }
        }

        private bool _purgeTags;
        public bool PurgeTags
        {
            get { return _purgeTags; }
            set
            {
                _purgeTags = value;
                OnPropertyChanged(nameof(PurgeTags));
            }
        }

        private bool _purgeFilters;
        public bool PurgeFilters
        {
            get { return _purgeFilters; }
            set
            {
                _purgeFilters = value;
                OnPropertyChanged(nameof(PurgeFilters));
            }
        }

        private bool _purgeViewTemplates;
        public bool PurgeViewTemplates
        {
            get { return _purgeViewTemplates; }
            set
            {
                _purgeViewTemplates = value;
                OnPropertyChanged(nameof(PurgeViewTemplates));
            }
        }

        private bool _purgeSheets;
        public bool PurgeSheets
        {
            get { return _purgeSheets; }
            set
            {
                _purgeSheets = value;
                OnPropertyChanged(nameof(PurgeSheets));
            }
        }

        private bool _purgeLineStyles;
        public bool PurgeLineStyles
        {
            get { return _purgeLineStyles; }
            set
            {
                _purgeLineStyles = value;
                OnPropertyChanged(nameof(PurgeLineStyles));
            }
        }

        private bool _purgeLinePatterns;
        public bool PurgeLinePatterns
        {
            get { return _purgeLinePatterns; }
            set
            {
                _purgeLinePatterns = value;
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private string _linePatternName;
        public string LinePatternName
        {
            get { return _linePatternName; }
            set
            {
                _linePatternName = value;
                OnPropertyChanged(nameof(LinePatternName));
            }
        }

        #endregion

        public PurgeM(ExternalCommandData commandData, string transactionName) : base(commandData, transactionName)
        {
        }

        #region IEXTERNALEVENTHANDLER

        public override void Execute(UIApplication app)
        {
            RunStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    RunReport = RunPurges();

                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                RunFailed = true;
                RunResult = ExceptionUtils.GetMessage(e);
            }
        }

        #endregion

        #region METHODS

        private DataSet RunPurges()
        {
            List<ReportMessage> reportMessageList = new List<ReportMessage>();

            if (PurgeRooms)
                reportMessageList.AddRange(PurgeRoomsNotPlaced());
            if (PurgeTags)
                reportMessageList.AddRange(PurgeTagsEmpty());
            if (PurgeFilters)
                reportMessageList.AddRange(PurgeFiltersUnused());
            if (PurgeViewTemplates)
                reportMessageList.AddRange(PurgeViewTemplatesUnused());
            if (PurgeSheets)
                reportMessageList.AddRange(PurgeSheetsEmpty());
            if (PurgeLineStyles)
                reportMessageList.AddRange(PurgeLineStylesUnused());
            if (PurgeLinePatterns)
                reportMessageList.AddRange(PurgeLinePatternsByName());

            return GetRunReport(reportMessageList);
        }

        /// <summary>
        /// Delete unplaced rooms in the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeRoomsNotPlaced()
        {
            ReportMessage reportMessage = new ReportMessage("Rooms not placed");

            ICollection<ElementId> rooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .WherePasses(new RoomFilter())
                .ToElements()
                .Cast<Room>()
                .Where(x => x.Location == null)
                .Select(x => x.Id)
                .ToList();

            _doc.Delete(rooms);

            reportMessage.MessageText = $"{rooms.Count} rooms deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete empty tags from the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeTagsEmpty()
        {
            ReportMessage reportMessage = new ReportMessage("Empty tags");

            ICollection<ElementId> tags = new FilteredElementCollector(_doc)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>()
                .Where(x => x.TagText.Length == 0)
                .Select(x => x.Id)
                .ToList();

            _doc.Delete(tags);

            reportMessage.MessageText = $"{tags.Count} tags deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete unused filters from the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeFiltersUnused()
        {
            ReportMessage reportMessage = new ReportMessage("Unused filters");

            IEnumerable<View> views = new FilteredElementCollector(_doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();
            ICollection<ElementId> filtersAll = new FilteredElementCollector(_doc)
                .OfClass(typeof(FilterElement))
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get views filters list
            List<ElementId> filtersUsed = new List<ElementId>();
            foreach (View view in views)
            {
                // Not all views support View/Graphics override
                if (view.ViewType == ViewType.AreaPlan || view.ViewType == ViewType.CeilingPlan
                    || view.ViewType == ViewType.Detail || view.ViewType == ViewType.DraftingView
                    || view.ViewType == ViewType.Elevation || view.ViewType == ViewType.EngineeringPlan
                    || view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.Section
                    || view.ViewType == ViewType.ThreeD)
                {
                    // Add to the list if it not contains those filters yet
                    ICollection<ElementId> viewFilters = view.GetFilters();
                    filtersUsed.AddRange(viewFilters.Where(x => !filtersUsed.Contains(x)));
                }
            }
            ICollection<ElementId> filtersUnused = filtersAll.Where(x => !filtersUsed.Contains(x)).ToList();

            _doc.Delete(filtersUnused);

            reportMessage.MessageText = $"{filtersUnused.Count} filters deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete unused view templates from the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeViewTemplatesUnused()
        {
            ReportMessage reportMessage = new ReportMessage("Unused view templates");

            IEnumerable<View> viewsAll = new FilteredElementCollector(_doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();

            IEnumerable<View> views = viewsAll.Where(x => x.IsTemplate == false);
            ICollection<ElementId> templateIds = viewsAll
                .Where(x => x.IsTemplate == true)
                .Select(x => x.Id)
                .ToList();

            foreach (View view in views)
            {
                if (templateIds.Contains(view.ViewTemplateId))
                    templateIds = templateIds.Where(x => x != view.ViewTemplateId).ToList();
            }

            _doc.Delete(templateIds);

            reportMessage.MessageText = $"{templateIds.Count} view templates deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete empty sheets witout any placed views from the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeSheetsEmpty()
        {
            ReportMessage reportMessage = new ReportMessage("Empty sheets");

            IEnumerable<ViewSheet> sheets = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ViewSheet>()
                .Where(x => x.GetAllPlacedViews().Count == 0)
                .Where(x => x.IsPlaceholder == false);

            // "Empty" sheets can contain schedules still, so filter sheets without schedules.
            IEnumerable<ElementId> schedulesSheets = new FilteredElementCollector(_doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ScheduleSheetInstance>()
                .Select(x => x.OwnerViewId);

            ICollection<ElementId> sheetsEmpty = sheets
                .Select(x => x.Id)
                .Where(x => !schedulesSheets.Contains(x))
                .ToList();

            _doc.Delete(sheetsEmpty);

            reportMessage.MessageText = $"{sheetsEmpty.Count} sheets deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete unused linestyles from the document.
        /// </summary>
        private IEnumerable<ReportMessage> PurgeLineStylesUnused()
        {
            ReportMessage reportMessage = new ReportMessage("Unused linestyles");

            ICollection<ElementId> lineStylesUnused = new List<ElementId>();

            // Get all used linestyles in the project.
            IEnumerable<ElementId> lineStylesUsed = new FilteredElementCollector(_doc)
                .OfClass(typeof(CurveElement))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<CurveElement>().ToList()
                .ConvertAll(x => x.LineStyle.Id)
                .Distinct();

            // Get all line styles in the project (but not built-in).
            CategoryNameMap lineStylesAllCnm = _doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
            ICollection<ElementId> lineStylesAll = new List<ElementId>();
            foreach (Category category in lineStylesAllCnm)
                if (category.Id.IntegerValue > 0)
                    lineStylesAll.Add(category.Id);

            lineStylesUnused = lineStylesAll
                .Where(x => !lineStylesUsed.Contains(x))
                .ToList();

            _doc.Delete(lineStylesUnused);

            reportMessage.MessageText = $"{lineStylesUnused.Count} linestyles deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        /// <summary>
        /// Delete line patterns by given part of the name.
        /// </summary>
        /// <param name="name">String to search in names.</param>
        private IEnumerable<ReportMessage> PurgeLinePatternsByName()
        {
            ReportMessage reportMessage = new ReportMessage("Line patterns");

            ICollection<ElementId> linePatterns = new FilteredElementCollector(_doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(LinePatternName))
                    .Select(x => x.Id)
                    .ToList();

            _doc.Delete(linePatterns);

            reportMessage.MessageText = $"{linePatterns.Count} line patterns with names containing \"{LinePatternName}\" deleted.";

            return new List<ReportMessage>() { reportMessage };
        }

        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages)
        {
            DataSet reportDataSet = new DataSet("reportDataSet");

            // Create DataTable
            DataTable reportDataTable = new DataTable("Purge");
            // Create 4 columns, and add them to the table
            DataColumn reportColumnCheck = new DataColumn("Purge", typeof(string));
            DataColumn reportColumnResult = new DataColumn("Result", typeof(string));

            reportDataTable.Columns.Add(reportColumnCheck);
            reportDataTable.Columns.Add(reportColumnResult);

            // Add the table to the DataSet
            reportDataSet.Tables.Add(reportDataTable);

            // Fill the table
            foreach (ReportMessage reportMessage in reportMessages)
            {
                DataRow dataRow = reportDataTable.NewRow();
                dataRow["Purge"] = reportMessage.MessageName;
                dataRow["Result"] = reportMessage.MessageText;
                reportDataTable.Rows.Add(dataRow);
            }

            return reportDataSet;
        }

        private protected override string GetRunResult() { return ""; }

        #endregion
    }
}