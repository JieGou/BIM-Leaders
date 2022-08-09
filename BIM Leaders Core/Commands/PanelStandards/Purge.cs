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
    [TransactionAttribute(TransactionMode.Manual)]
    public class Purge : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int countRooms = 0;
            int countTags = 0;
            int countFilters = 0;
            int countViewTemplates = 0;
            int countSheets = 0;
            int countLineStyles = 0;
            int countLinePatterns = 0;

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
                        countRooms = PurgeRoomsNotPlaced(doc);
                    if (inputTags)
                        countTags = PurgeTagsEmpty(doc);
                    if (inputFilters)
                        countFilters = PurgeFiltersUnused(doc);
                    if (inputViewTemplates)
                        countViewTemplates = PurgeViewTemplatesUnused(doc);
                    if (inputSheets)
                        countSheets = PurgeSheetsEmpty(doc);
                    if (inputLineStyles)
                        countLineStyles = PurgeLineStylesUnused(doc);
                    if (inputLinePatterns)
                        countLinePatterns = PurgeLinePatterns(doc, inputLinePatternsName);

                    trans.Commit();
                }
                ShowResult(countLineStyles, countFilters, countRooms, countTags, countViewTemplates, countSheets, countLinePatterns, inputLinePatternsName);

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
        /// <returns>Count of deleted unplaced rooms.</returns>
        private static int PurgeRoomsNotPlaced(Document doc)
        {
            int count = 0;

            ICollection<ElementId> rooms = new FilteredElementCollector(doc)
                .OfClass(typeof(SpatialElement))
                .WherePasses(new RoomFilter())
                .ToElements()
                .Cast<Room>()
                .Where(x => x.Location == null)
                .Select(x => x.Id)
                .ToList();
            
            count = rooms.Count;

            doc.Delete(rooms);

            return count;
        }

        /// <summary>
        /// Delete empty tags from the document.
        /// </summary>
        /// <returns>Count of deleted empty tags.</returns>
        private static int PurgeTagsEmpty(Document doc)
        {
            int count = 0;

            ICollection<ElementId> tags = new FilteredElementCollector(doc)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>()
                .Where(x => x.TagText.Length == 0)
                .Select(x => x.Id)
                .ToList();

            count = tags.Count;

            doc.Delete(tags);

            return count;
        }

        /// <summary>
        /// Delete unused filters from the document.
        /// </summary>
        /// <returns>Count of deleted unused filters.</returns>
        private static int PurgeFiltersUnused(Document doc)
        {
            int count = 0;

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

            count = filtersUnused.Count;

            doc.Delete(filtersUnused);

            return count;
        }

        /// <summary>
        /// Delete unused view templates from the document.
        /// </summary>
        /// <returns>Count of deleted unused view templates.</returns>
        private static int PurgeViewTemplatesUnused(Document doc)
        {
            int count = 0;

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

            count = templateIds.Count;

            doc.Delete(templateIds);

            return count;
        }

        /// <summary>
        /// Delete empty sheets witout any placed views from the document.
        /// </summary>
        /// <returns>Count of deleted empty sheets.</returns>
        private static int PurgeSheetsEmpty(Document doc)
        {
            int count = 0;

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

            count = sheetsEmpty.Count;

            doc.Delete(sheetsEmpty);

            return count;
        }

        /// <summary>
        /// Delete unused linestyles from the document.
        /// </summary>
        /// <returns>Count of deleted unused linestyles.</returns>
        private static int PurgeLineStylesUnused(Document doc)
        {
            int count = 0;

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

            count = lineStylesUnused.Count;

            doc.Delete(lineStylesUnused);

            return count;
        }

        /// <summary>
        /// Delete line patterns by given part of the name.
        /// </summary>
        /// <param name="name">String to search in names.</param>
        /// <returns>Count of deleted line patterns.</returns>
        private static int PurgeLinePatterns(Document doc, string name)
        {
            int count = 0;

            ICollection<ElementId> linePatterns = new FilteredElementCollector(doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(name))
                    .Select(x => x.Id)
                    .ToList();

            count = linePatterns.Count;

            doc.Delete(linePatterns);

            return count;
        }

        private static void ShowResult(int countLineStyles, int countFilters, int countRooms, int countTags, int countViewTemplates, int countSheets, int countLinePatterns, string inputLinePatternsName)
        {
            // Show result
            string text = "";
            if (countLineStyles + countFilters == 0)
                text = "No elements deleted";
            else
            {
                if (countRooms > 0)
                    text += $"{countRooms} non-placed rooms deleted";
                if (countTags > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countTags} empty tags deleted";
                }
                if (countFilters > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countFilters} unused filters deleted";
                }
                if (countViewTemplates > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countViewTemplates} unused view templates deleted";
                }
                if (countSheets > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countSheets} empty sheets deleted";
                }
                if (countLineStyles > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countLineStyles} unused linestyles deleted";
                }
                if (countLinePatterns > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countLinePatterns} line patterns with names included \"{inputLinePatternsName}\" deleted";
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