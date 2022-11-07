using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class PurgeM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private int _countRoomsNotPlaced;
        private int _countTagsEmpty;
        private int _countFiltersUnused;
        private int _countViewTemplatesUnused;
        private int _countSheetsEmpty;
        private int _countLineStylesUnused;
        private int _countLinePatterns;

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private string _transactionName;
        public string TransactionName
        {
            get { return _transactionName; }
            set
            {
                _transactionName = value;
                OnPropertyChanged(nameof(TransactionName));
            }
        }

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

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            {
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        #endregion

        public PurgeM(ExternalCommandData commandData, string transactionName)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            TransactionName = transactionName;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TransactionName;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    RunPurges();

                    trans.Commit();
                }

                GetRunResult();
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }
        }

        #endregion

        #region METHODS

        private void RunPurges()
        {
            if (PurgeRooms)
                PurgeRoomsNotPlaced();
            if (PurgeTags)
                PurgeTagsEmpty();
            if (PurgeFilters)
                PurgeFiltersUnused();
            if (PurgeViewTemplates)
                PurgeViewTemplatesUnused();
            if (PurgeSheets)
                PurgeSheetsEmpty();
            if (PurgeLineStyles)
                PurgeLineStylesUnused();
            if (PurgeLinePatterns)
                PurgeLinePatternsByName();
        }

        /// <summary>
        /// Delete unplaced rooms in the document.
        /// </summary>
        private void PurgeRoomsNotPlaced()
        {
            ICollection<ElementId> rooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .WherePasses(new RoomFilter())
                .ToElements()
                .Cast<Room>()
                .Where(x => x.Location == null)
                .Select(x => x.Id)
                .ToList();

            _countRoomsNotPlaced = rooms.Count;

            _doc.Delete(rooms);
        }

        /// <summary>
        /// Delete empty tags from the document.
        /// </summary>
        private void PurgeTagsEmpty()
        {
            ICollection<ElementId> tags = new FilteredElementCollector(_doc)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>()
                .Where(x => x.TagText.Length == 0)
                .Select(x => x.Id)
                .ToList();

            _countTagsEmpty = tags.Count;

            _doc.Delete(tags);
        }

        /// <summary>
        /// Delete unused filters from the document.
        /// </summary>
        private void PurgeFiltersUnused()
        {
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

            _countFiltersUnused = filtersUnused.Count;

            _doc.Delete(filtersUnused);
        }

        /// <summary>
        /// Delete unused view templates from the document.
        /// </summary>
        private void PurgeViewTemplatesUnused()
        {
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

            _countViewTemplatesUnused = templateIds.Count;

            _doc.Delete(templateIds);
        }

        /// <summary>
        /// Delete empty sheets witout any placed views from the document.
        /// </summary>
        private void PurgeSheetsEmpty()
        {
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

            _countSheetsEmpty = sheetsEmpty.Count;

            _doc.Delete(sheetsEmpty);
        }

        /// <summary>
        /// Delete unused linestyles from the document.
        /// </summary>
        private void PurgeLineStylesUnused()
        {
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

            _countLineStylesUnused = lineStylesUnused.Count;

            _doc.Delete(lineStylesUnused);
        }

        /// <summary>
        /// Delete line patterns by given part of the name.
        /// </summary>
        /// <param name="name">String to search in names.</param>
        private void PurgeLinePatternsByName()
        {
            ICollection<ElementId> linePatterns = new FilteredElementCollector(_doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(LinePatternName))
                    .Select(x => x.Id)
                    .ToList();

            _doc.Delete(linePatterns);

            _countLinePatterns = linePatterns.Count;
        }

        private void GetRunResult()
        {
            if (_countLineStylesUnused + _countFiltersUnused == 0)
                RunResult = "No elements deleted";
            else
            {
                if (_countRoomsNotPlaced > 0)
                    RunResult += $"{_countRoomsNotPlaced} non-placed rooms deleted";
                if (_countTagsEmpty > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countTagsEmpty} empty tags deleted";
                }
                if (_countFiltersUnused > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countFiltersUnused} unused filters deleted";
                }
                if (_countViewTemplatesUnused > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countViewTemplatesUnused} unused view templates deleted";
                }
                if (_countSheetsEmpty > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countSheetsEmpty} empty sheets deleted";
                }
                if (_countLineStylesUnused > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countLineStylesUnused} unused linestyles deleted";
                }
                if (_countLinePatterns > 0)
                {
                    if (RunResult.Length > 0)
                        RunResult += ", ";
                    RunResult += $"{_countLinePatterns} line patterns with names included \"{LinePatternName}\" deleted";
                }
                RunResult += ".";
            }
        }

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}