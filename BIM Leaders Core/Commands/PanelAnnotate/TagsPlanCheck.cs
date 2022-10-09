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
    public class TagsPlanCheck : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string filterName = "Check - Tags";

            try
            {
                TagsPlanCheckForm form = new TagsPlanCheckForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                TagsPlanCheckVM data = form.DataContext as TagsPlanCheckVM;
                Color filterColor = new Color(data.ResultColor.R, data.ResultColor.G, data.ResultColor.B);

                List<ElementId> elementIds = GetUntaggedElementIds(doc);
                List<ElementId> railingIds = GetUntaggedRailingIds(doc);

                using (Transaction trans = new Transaction(doc, "Create Filter for non-dimensioned Walls"))
                {
                    trans.Start();

                    ElementId filter1Id = CreateFilter(doc, filterName, elementIds);
                    doc.Regenerate();
                    SetupFilter(doc, filter1Id, filterColor);

                    trans.Commit();
                }
                uidoc.Selection.SetElementIds(railingIds);

                ShowResult(elementIds.Count, railingIds.Count);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get elements that have no tags on active view.
        /// </summary>
        /// <returns>List<ElementId> of untagged elements.</returns>
        private static List<ElementId> GetUntaggedElementIds(Document doc)
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
            IEnumerable<ElementId> elementsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get Tags.
            IEnumerable<IndependentTag> tagsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
                    if (tag.TaggedLocalElementId == elementId)
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
                }
            }

            return elementIds;
        }

        /// <summary>
        /// Get railings that have no tags on active view.
        /// </summary>
        /// <returns>List<ElementId> of untagged railings.</returns>
        private static List<ElementId> GetUntaggedRailingIds(Document doc)
        {
            List<ElementId> elementIds = new List<ElementId>();

            // Get elements.
            IEnumerable<ElementId> elementsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(Railing))
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Get Tags.
            IEnumerable<IndependentTag> tagsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
                    if (tag.TaggedLocalElementId == elementId)
                    {
                        elementIds.Remove(elementId);
                        continue;
                    }
                }
            }

            return elementIds;
        }

        /// <summary>
        /// Create a selection filter with given set of elements. Applies created filter to the active view.
        /// </summary>
        /// <returns>Created filter element Id.</returns>
        private static ElementId CreateFilter(Document doc, string filterName, List<ElementId> elementIds)
        {
            View view = doc.ActiveView;

            // Checking if filter already exists
            IEnumerable<Element> filters = new FilteredElementCollector(doc)
                .OfClass(typeof(SelectionFilterElement))
                .ToElements();
            foreach (Element element in filters)
                if (element.Name == filterName)
                    doc.Delete(element.Id);

            SelectionFilterElement filter = SelectionFilterElement.Create(doc, filterName);
            filter.SetElementIds(elementIds);

            // Add the filter to the view
            ElementId filterId = filter.Id;
            view.AddFilter(filterId);

            return filterId;
        }

        /// <summary>
        /// Change filter settings. Must be applied after regeneration when filter is new.
        /// </summary>
        private static void SetupFilter(Document doc, ElementId filterId, Color filterColor)
        {
            View view = doc.ActiveView;

            // Get solid pattern.
            ElementId patternId = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .ToElements()
                .Cast<FillPatternElement>()
                .Where(x => x.GetFillPattern().IsSolidFill)
                .First().Id;

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

        private static void ShowResult(int elementIds, int railingIds)
        {
            // Show result
            string text = "";
            if (elementIds + railingIds == 0)
                text = "All elements are tagged";
            else
            {
                if (elementIds > 0)
                    text += $"{elementIds} elements added to filter \"Check - Tags\".";
                if (railingIds > 0)
                {
                    if (text.Length > 0)
                        text += " ";
                    text += $"Railings filters not usable, so {railingIds} untagged railings were selected.";
                }
            }

            TaskDialog.Show("Tags Plan Check", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(TagsPlanCheck).Namespace + "." + nameof(TagsPlanCheck);
        }
    }
}
