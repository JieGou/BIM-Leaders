using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Core
{
    internal static class ViewFilterUtils
    {
        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// Filters order are not introduced in the API (only get).
        /// </summary>
        /// <returns>Created filter.</returns>
        internal static SelectionFilterElement CreateSelectionFilter(Document doc, string filterName, ICollection<Element> elements)
        {
            SelectionFilterElement filter;

            View view = doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(doc)
                .OfClass(typeof(SelectionFilterElement))
                .ToElements();
            foreach (Element element in filters)
                if (element.Name == filterName)
                    doc.Delete(element.Id);

            filter = SelectionFilterElement.Create(doc, filterName);
            filter.SetElementIds(elements.Select(x => x.Id).ToList());

            // Add the filter to the view
            ElementId filterId = filter.Id;
            view.AddFilter(filterId);

            doc.Regenerate();

            return filter;
        }

        /// <summary>
        /// Change filter settings. Must be applied after regeneration when filter is new.
        /// </summary>
        internal static void SetupFilter(Document doc, Element filter, Color filterColor)
        {
            View view = doc.ActiveView;

            // Get solid pattern.
            IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
            ElementId pattern = patterns.First().Id;
            foreach (Element element in patterns)
                if (element.Name == "<Solid fill>")
                    pattern = element.Id;

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filter.Id);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(pattern);
            view.SetFilterOverrides(filter.Id, overrideSettings);
        }
    }
}