using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    public static class ViewFilterUtils
    {
        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// Filters order are not introduced in the API (only get).
        /// </summary>
        /// <returns>Created filter.</returns>
        public static SelectionFilterElement CreateSelectionFilter(Document doc, string filterName, ICollection<Element> elements)
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
        public static void SetupFilter(Document doc, Element filter, Color filterColor)
        {
            View view = doc.ActiveView;

            // Get solid pattern.
            FillPatternElement pattern = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First();

            // Use the existing graphics settings, and change the color.
            OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filter.Id);
            overrideSettings.SetCutForegroundPatternColor(filterColor);
            overrideSettings.SetCutForegroundPatternId(pattern.Id);
            view.SetFilterOverrides(filter.Id, overrideSettings);
        }
    }
}