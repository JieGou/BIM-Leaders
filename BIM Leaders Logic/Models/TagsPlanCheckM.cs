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
    public class TagsPlanCheckM : BaseModel
    {
        private int _countUntaggedElements;
        private int _countUntaggedRailings;

        private const string FILTER_NAME = "Check - Tags";

        #region PROPERTIES

        private System.Windows.Media.Color _filterColorSystem;
        public System.Windows.Media.Color FilterColorSystem
        {
            get { return _filterColorSystem; }
            set
            {
                _filterColorSystem = value;
                OnPropertyChanged(nameof(FilterColorSystem));
            }
        }

        private Autodesk.Revit.DB.Color _filterColor;
        public Autodesk.Revit.DB.Color FilterColor
        {
            get { return _filterColor; }
            set
            {
                _filterColor = value;
                OnPropertyChanged(nameof(FilterColor));
            }
        }

        #endregion

        public TagsPlanCheckM(
            ExternalCommandData commandData,
            string transactionName,
            Action<string, RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            ConvertUserInput();

            List<ElementId> elementIds = GetUntaggedElementIds();
            List<ElementId> railingIds = GetUntaggedRailingIds();

            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                ElementId filter1Id = CreateFilter(elementIds);
                _doc.Regenerate();
                SetupFilter(filter1Id);

                trans.Commit();
            }
            _uidoc.Selection.SetElementIds(railingIds);

            _result.Result = GetRunResult();
        }

        private void ConvertUserInput()
        {
            FilterColor = new Autodesk.Revit.DB.Color(
                FilterColorSystem.R,
                FilterColorSystem.G,
                FilterColorSystem.B);
        }

        /// <summary>
        /// Get elements that have no tags on active view.
        /// </summary>
        /// <returns>List<ElementId> of untagged elements.</returns>
        private List<ElementId> GetUntaggedElementIds()
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
        private List<ElementId> GetUntaggedRailingIds()
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
        private ElementId CreateFilter(List<ElementId> elementIds)
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
        private void SetupFilter(ElementId filterId)
        {
            View view = _doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
            overrideSettings.SetCutForegroundPatternColor(FilterColor);
            overrideSettings.SetCutForegroundPatternId(patternId);
            overrideSettings.SetSurfaceForegroundPatternColor(FilterColor);
            overrideSettings.SetSurfaceForegroundPatternId(patternId);
            overrideSettings.SetCutLineColor(FilterColor);
            overrideSettings.SetProjectionLineColor(FilterColor);
            view.SetFilterOverrides(filterId, overrideSettings);
        }

        private protected override string GetRunResult()
        {
            string text = "";

            if (_countUntaggedElements + _countUntaggedRailings == 0)
                text = "All elements are tagged.";
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

            return text;
        }

        #endregion
    }
}