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
        private static int _countRooms;
        private static int _countTags;
        private static int _countFilters;
        private static int _countViewTemplates;
        private static int _countSheets;
        private static int _countLineStyles;
        private static int _countLinePatterns;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                PurgeForm form = new PurgeForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                PurgeData data = form.DataContext as PurgeData;

                // Getting input data from user
                bool inputRooms = data.ResultRooms;
                bool inputTags = data.ResultTags;
                bool inputFilters = data.ResultFilters;
                bool inputViewTemplates = data.ResultViewTemplates;
                bool inputSheets = data.ResultSheets;
                bool inputLineStyles = data.ResultLineStyles;
                bool inputLinePatterns = data.ResultLinePatterns;
                string inputLinePatternsName = data.ResultLinePatternsName;

                using (Transaction trans = new Transaction(doc, "Purge"))
                {
                    trans.Start();

                    if (inputRooms)
                        PurgeRoomsNotPlaced(doc);
                    if (inputTags)
                        PurgeTagsEmpty(doc);
                    if (inputFilters)
                        PurgeFiltersUnused(doc);
                    if (inputViewTemplates)
                        PurgeViewTemplatesUnused(doc);
                    if (inputSheets)
                        PurgeSheetsEmpty(doc);
                    if (inputLineStyles)
                        PurgeLineStylesUnused(doc);
                    if (inputLinePatterns)
                        PurgeLinePatterns(doc, inputLinePatternsName);

                    trans.Commit();
                }
                ShowResult(inputLinePatternsName);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Delete unplaced rooms in the document.
        /// </summary>
        private static void PurgeRoomsNotPlaced(Document doc)
        {
            ICollection<ElementId> rooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .WherePasses(new RoomFilter())
                .ToElements()
                .Cast<Room>()
                .Where(x => x.Location == null)
                .Select(x => x.Id)
                .ToList();
            
            _countRooms = rooms.Count;

            doc.Delete(rooms);
        }

        /// <summary>
        /// Delete empty tags from the document.
        /// </summary>
        private static void PurgeTagsEmpty(Document doc)
        {
            ICollection<ElementId> tags = new FilteredElementCollector(doc)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>()
                .Where(x => x.TagText.Length == 0)
                .Select(x => x.Id)
                .ToList();

            _countTags = tags.Count;

            doc.Delete(tags);
        }

        /// <summary>
        /// Delete unused filters from the document.
        /// </summary>
        private static void PurgeFiltersUnused(Document doc)
        {
            IEnumerable<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<View>();
            ICollection<ElementId> filtersAll = new FilteredElementCollector(doc)
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

            _countFilters = filtersUnused.Count;

            doc.Delete(filtersUnused);
        }

        /// <summary>
        /// Delete unused view templates from the document.
        /// </summary>
        private static void PurgeViewTemplatesUnused(Document doc)
        {
            IEnumerable<View> viewsAll = new FilteredElementCollector(doc)
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

            _countViewTemplates = templateIds.Count;

            doc.Delete(templateIds);
        }

        /// <summary>
        /// Delete empty sheets witout any placed views from the document.
        /// </summary>
        private static void PurgeSheetsEmpty(Document doc)
        {
            IEnumerable<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ViewSheet>()
                .Where(x => x.GetAllPlacedViews().Count == 0)
                .Where(x => x.IsPlaceholder == false);

            // "Empty" sheets can contain schedules still, so filter sheets without schedules.
            IEnumerable<ElementId> schedulesSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ScheduleSheetInstance))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<ScheduleSheetInstance>()
                .Select(x => x.OwnerViewId);

            ICollection<ElementId> sheetsEmpty = sheets
                .Select(x => x.Id)
                .Where(x => !schedulesSheets.Contains(x))
                .ToList();

            _countSheets = sheetsEmpty.Count;

            doc.Delete(sheetsEmpty);
        }

        /// <summary>
        /// Delete unused linestyles from the document.
        /// </summary>
        private static void PurgeLineStylesUnused(Document doc)
        {
            ICollection<ElementId> lineStylesUnused = new List<ElementId>();

            // Get all used linestyles in the project.
            IEnumerable<ElementId> lineStylesUsed = new FilteredElementCollector(doc)
                .OfClass(typeof(CurveElement))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<CurveElement>().ToList()
                .ConvertAll(x => x.LineStyle.Id)
                .Distinct();

            // Get all line styles in the project (but not built-in).
            CategoryNameMap lineStylesAllCnm = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
            ICollection<ElementId> lineStylesAll = new List<ElementId>();
            foreach (Category category in lineStylesAllCnm)
                if (category.Id.IntegerValue > 0)
                    lineStylesAll.Add(category.Id);

            lineStylesUnused = lineStylesAll
                .Where(x => !lineStylesUsed.Contains(x))
                .ToList();

            _countLineStyles = lineStylesUnused.Count;

            doc.Delete(lineStylesUnused);
        }

        /// <summary>
        /// Delete line patterns by given part of the name.
        /// </summary>
        /// <param name="name">String to search in names.</param>
        private static void PurgeLinePatterns(Document doc, string name)
        {
            ICollection<ElementId> linePatterns = new FilteredElementCollector(doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(name))
                    .Select(x => x.Id)
                    .ToList();

            _countLinePatterns = linePatterns.Count;

            doc.Delete(linePatterns);
        }

        private static void ShowResult(string inputLinePatternsName)
        {
            // Show result
            string text = "";
            if (_countLineStyles + _countFilters == 0)
                text = "No elements deleted";
            else
            {
                if (_countRooms > 0)
                    text += $"{_countRooms} non-placed rooms deleted";
                if (_countTags > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countTags} empty tags deleted";
                }
                if (_countFilters > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countFilters} unused filters deleted";
                }
                if (_countViewTemplates > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countViewTemplates} unused view templates deleted";
                }
                if (_countSheets > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countSheets} empty sheets deleted";
                }
                if (_countLineStyles > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countLineStyles} unused linestyles deleted";
                }
                if (_countLinePatterns > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countLinePatterns} line patterns with names included \"{inputLinePatternsName}\" deleted";
                }
                text += ".";
            }

            TaskDialog.Show("Purge", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Purge).Namespace + "." + nameof(Purge);
        }
    }
}