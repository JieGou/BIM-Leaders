using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class NamesChange : IExternalCommand
    {
        private static int _countNamesChanged;
        private static NamesChangeData _inputData;

        private const string TRANSACTION_NAME = "Change Names";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    ReplaceNames(doc);

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

        private static NamesChangeData GetUserInput()
        {
            NamesChangeForm form = new NamesChangeForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as NamesChangeData;
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
        private static void ReplaceNames(Document doc)
        {
            List<Type> types = Categories.GetTypesList(_inputData.ResultCategories);

            ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

            IEnumerable<Element> elements = new FilteredElementCollector(doc)
                .WherePasses(elementMulticlassFilter)
                .ToElements();

            // Prefix location replacement
            if (_inputData.ResultPartPrefix)
                ReplaceNamesPrefix(elements);
            else if (_inputData.ResultPartSuffix)
                ReplaceNamesSuffix(elements);
            else
                ReplaceNamesCenter(elements);
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private static void ReplaceNamesPrefix(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.StartsWith(_inputData.ResultSubstringOld))
                {
                    string nameNew = name.TrimStart(_inputData.ResultSubstringOld.ToCharArray());
                    nameNew = _inputData.ResultSubstringNew += nameNew;
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private static void ReplaceNamesSuffix(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.EndsWith(_inputData.ResultSubstringOld))
                {
                    string nameNew = name.TrimEnd(_inputData.ResultSubstringOld.ToCharArray());
                    nameNew += _inputData.ResultSubstringNew;
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        /// <summary>
        /// Replace substring in names of given elements.
        /// </summary>
        private static void ReplaceNamesCenter(IEnumerable<Element> elements)
        {
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.Contains(_inputData.ResultSubstringOld))
                {
                    string nameNew = name.Replace(_inputData.ResultSubstringOld, _inputData.ResultSubstringNew);
                    element.Name = nameNew;

                    _countNamesChanged++;
                }
            }
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countNamesChanged == 0)
                ? "No names changed"
                : $"{_countNamesChanged} names changed";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
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
