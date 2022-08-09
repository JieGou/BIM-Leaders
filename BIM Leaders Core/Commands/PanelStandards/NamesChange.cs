using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class NamesChange : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int count = 0;

            try
            {
                NamesChangeForm form = new NamesChangeForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                NamesChangeData data = form.DataContext as NamesChangeData;

                // Getting input data from user
                string inputSubstringOld = data.ResultSubstringOld;
                string inputSubstringNew = data.ResultSubstringNew;
                bool inputPartPrefix = data.ResultPartPrefix;
                bool inputPartSuffix = data.ResultPartSuffix;
                List<bool> categories = data.ResultCategories;

                using (Transaction trans = new Transaction(doc, "Change Names Prefix"))
                {
                    trans.Start();

                    count += ReplaceNames(doc, inputSubstringOld, inputSubstringNew, inputPartPrefix, inputPartSuffix, categories);

                    trans.Commit();
                }
                ShowResult(count);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        /// <summary>
        /// Replace substring in names of elements of given types.
        /// <para>
        /// <c>int count = CountPrefixes(doc, "OLD", "NEW", typeof(GutterType));</c>
        /// </para>
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <param name="substringOld">String to search.</param>
        /// <param name="substringNew">String to replace with.</param>
        /// <param name="inputPartPrefix">If true, replace prefix part.</param>
        /// <param name="inputPartSuffix">If true, replace suffix part.</param>
        /// <param name="types">Sytem.Type of needed DB classes (categories).</param>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNames(Document doc, string substringOld, string substringNew, bool inputPartPrefix, bool inputPartSuffix, List<bool> inputCategories)
        {
            int count = 0;

            List<Type> types = Categories.GetTypesList(inputCategories);

            ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

            IEnumerable<Element> elements = new FilteredElementCollector(doc)
                .WherePasses(elementMulticlassFilter)
                .ToElements();

            // Prefix location replacement
            if (inputPartPrefix)
                count = ReplaceNamesPrefix(elements, substringOld, substringNew);
            else if (inputPartSuffix)
                count = ReplaceNamesSuffix(elements, substringOld, substringNew);
            else
                count = ReplaceNamesCenter(elements, substringOld, substringNew);

            return count;
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNamesPrefix(IEnumerable<Element> elements, string substringOld, string substringNew)
        {
            int count = 0;

            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.StartsWith(substringOld))
                {
                    string nameNew = name.TrimStart(substringOld.ToCharArray());
                    nameNew = substringNew += nameNew;
                    element.Name = nameNew;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNamesSuffix(IEnumerable<Element> elements, string substringOld, string substringNew)
        {
            int count = 0;

            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.EndsWith(substringOld))
                {
                    string nameNew = name.TrimEnd(substringOld.ToCharArray());
                    nameNew += substringNew;
                    element.Name = nameNew;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNamesCenter(IEnumerable<Element> elements, string substringOld, string substringNew)
        {
            int count = 0;

            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.Contains(substringOld))
                {
                    string nameNew = name.Replace(substringOld, substringNew);
                    element.Name = nameNew;
                    count++;
                }
            }

            return count;
        }

        private static void ShowResult(int count)
        {
            // Show result
            string text = (count == 0)
                ? "No names changed"
                : $"{count} names changed";
            
            TaskDialog.Show("Names Change", text);
        }

        /// <summary>
        /// Gets the full namespace path to this command
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(NamesChange).Namespace + "." + nameof(NamesChange);
        }
    }
}
