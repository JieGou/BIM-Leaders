using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using Autodesk.Revit.DB.Architecture;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Purge : IExternalCommand
    {
        private static Document _doc;
        private static PurgeData _inputData;
        private static int _countRoomsNotPlaced;
        private static int _countTagsEmpty;
        private static int _countFiltersUnused;
        private static int _countViewTemplatesUnused;
        private static int _countSheetsEmpty;
        private static int _countLineStylesUnused;
        private static int _countLinePatterns;

        private const string TRANSACTION_NAME = "Purge";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    RunPurges();

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

        private static PurgeData GetUserInput()
        {
            PurgeForm form = new PurgeForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as PurgeData;
        }

        private static void RunPurges()
        {
            if (_inputData.ResultRooms)
                PurgeRoomsNotPlaced();
            if (_inputData.ResultTags)
                PurgeTagsEmpty();
            if (_inputData.ResultFilters)
                PurgeFiltersUnused();
            if (_inputData.ResultViewTemplates)
                PurgeViewTemplatesUnused();
            if (_inputData.ResultSheets)
                PurgeSheetsEmpty();
            if (_inputData.ResultLineStyles)
                PurgeLineStylesUnused();
            if (_inputData.ResultLinePatterns)
                PurgeLinePatterns();
        }

        /// <summary>
        /// Delete unplaced rooms in the document.
        /// </summary>
        private static void PurgeRoomsNotPlaced()
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
        private static void PurgeTagsEmpty()
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
        private static void PurgeFiltersUnused()
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
        private static void PurgeViewTemplatesUnused()
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
        private static void PurgeSheetsEmpty()
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
        private static void PurgeLineStylesUnused()
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
        private static void PurgeLinePatterns()
        {
            ICollection<ElementId> linePatterns = new FilteredElementCollector(_doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(_inputData.ResultLinePatternsName))
                    .Select(x => x.Id)
                    .ToList();

            _doc.Delete(linePatterns);

            _countLinePatterns = linePatterns.Count;
        }

        private static void ShowResult()
        {
            // Show result
            string text = "";
            if (_countLineStylesUnused + _countFiltersUnused == 0)
                text = "No elements deleted";
            else
            {
                if (_countRoomsNotPlaced > 0)
                    text += $"{_countRoomsNotPlaced} non-placed rooms deleted";
                if (_countTagsEmpty > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countTagsEmpty} empty tags deleted";
                }
                if (_countFiltersUnused > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countFiltersUnused} unused filters deleted";
                }
                if (_countViewTemplatesUnused > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countViewTemplatesUnused} unused view templates deleted";
                }
                if (_countSheetsEmpty > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countSheetsEmpty} empty sheets deleted";
                }
                if (_countLineStylesUnused > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countLineStylesUnused} unused linestyles deleted";
                }
                if (_countLinePatterns > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countLinePatterns} line patterns with names included \"{_inputData.ResultLinePatternsName}\" deleted";
                }
                text += ".";
            }

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Purge).Namespace + "." + nameof(Purge);
        }
    }
}