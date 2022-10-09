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
    public class TagsPlanCheck : IExternalCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static TagsPlanCheckVM _inputData;
        private static int _countUntaggedElements;
        private static int _countUntaggedRailings;

        private const string TRANSACTION_NAME = "Tags Plan Check";
        private const string FILTER_NAME = "Check - Tags";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                List<ElementId> elementIds = GetUntaggedElementIds();
                List<ElementId> railingIds = GetUntaggedRailingIds();

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    ElementId filter1Id = CreateFilter(elementIds);
                    _doc.Regenerate();
                    SetupFilter(filter1Id);

                    trans.Commit();
                }
                _uidoc.Selection.SetElementIds(railingIds);

                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static TagsPlanCheckVM GetUserInput()
        {
            TagsPlanCheckForm form = new TagsPlanCheckForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as TagsPlanCheckVM;
        }

        /// <summary>
        /// Get elements that have no tags on active view.
        /// </summary>
        /// <returns>List<ElementId> of untagged elements.</returns>
        private static List<ElementId> GetUntaggedElementIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Filter.
            List<BuiltInCategory> categories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows,
            };
            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(categories);

            // Get elements.
            IEnumerable<ElementId> elementsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get Tags.
            IEnumerable<IndependentTag> tagsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>();

            // Iterate.
            elementIds = elementsAll.ToList();
            foreach (IndependentTag tag in tagsAll)
            {
                foreach (ElementId elementId in elementsAll)
                {
#if VERSION2020 || VERSION2021 || VERSION2022
                    if (tag.TaggedLocalElementId == elementId)
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
#else
                    if (tag.GetTaggedLocalElementIds().Contains(elementId))
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
#endif
                }
            }

            _countUntaggedElements = elementIds.Count;

            return elementIds;
        }

        /// <summary>
        /// Get railings that have no tags on active view.
        /// </summary>
        /// <returns>List<ElementId> of untagged railings.</returns>
        private static List<ElementId> GetUntaggedRailingIds()
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Get elements.
            IEnumerable<ElementId> elementsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Railing))
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get Tags.
            IEnumerable<IndependentTag> tagsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(IndependentTag))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<IndependentTag>();

            // Iterate.
            elementIds = elementsAll.ToList();
            foreach (IndependentTag tag in tagsAll)
            {
                foreach (ElementId elementId in elementsAll)
                {
#if VERSION2020 || VERSION2021 || VERSION2022
                    if (tag.TaggedLocalElementId == elementId)
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
#else
                    if (tag.GetTaggedLocalElementIds().Contains(elementId))
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
#endif
                }
            }

            _countUntaggedRailings = elementIds.Count;

            return elementIds;
        }

        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// </summary>
        /// <returns>Created filter element Id.</returns>
        private static ElementId CreateFilter(List<ElementId> elementIds)
        {
            View view = _doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(_doc)
                .OfClass(typeof(SelectionFilterElement))
                .ToElements();
            foreach (Element element in filters)
                if (element.Name == FILTER_NAME)
                    _doc.Delete(element.Id);

            SelectionFilterElement filter = SelectionFilterElement.Create(_doc, FILTER_NAME);
            filter.SetElementIds(elementIds);

            // Add the filter to the view
            ElementId filterId = filter.Id;
            view.AddFilter(filterId);

            return filterId;
        }

        /// <summary>
        /// Change filter settings. Must be applied after regeneration when filter is new.
        /// </summary>
        private static void SetupFilter(ElementId filterId)
        {
            View view = _doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

            Color filterColor = new Color(_inputData.ResultColor.R, _inputData.ResultColor.G, _inputData.ResultColor.B);

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(patternId);
            overrideSettings.SetSurfaceForegroundPatternColor(filterColor);
            overrideSettings.SetSurfaceForegroundPatternId(patternId);
            overrideSettings.SetCutLineColor(filterColor);
            overrideSettings.SetProjectionLineColor(filterColor);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        private static void ShowResult()
        {
            // Show result
            string text = "";
            if (_countUntaggedElements + _countUntaggedRailings == 0)
                text = "All elements are tagged";
            else
            {
                if (_countUntaggedElements > 0)
                    text += $"{_countUntaggedElements} elements added to filter \"Check - Tags\".";
                if (_countUntaggedRailings > 0)
                {
                    if (text.Length > 0)
                        text += " ";
                    text += $"Railings filters not usable, so {_countUntaggedRailings} untagged railings were selected.";
                }
            }

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(TagsPlanCheck).Namespace + "." + nameof(TagsPlanCheck);
        }
    }
}
