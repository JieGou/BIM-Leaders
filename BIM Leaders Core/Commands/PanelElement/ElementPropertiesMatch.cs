using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Diagnostics;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ElementPropertiesMatch : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Match instance properties";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static UIDocument _uidoc;
        private static Document _doc;
        private static int _countPropertiesMatched;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            _runStarted = true;

            try
            {
                // Pick first element
                Reference reference0 = _uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element elementFrom = _doc.GetElement(reference0.ElementId);

                // Pick second element
                SelectionFilterByCategory category = new SelectionFilterByCategory(elementFrom.Category.Name);

                Reference reference1 = _uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, category);
                Element elementTo = _doc.GetElement(reference1.ElementId);

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    MatchProperties(elementFrom, elementTo);

                    trans.Commit();
                }

                _runResult = GetRunResult();

                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Copy all instance properties values from one element to other.
        /// </summary>
        private static void MatchProperties(Element elementFrom, Element elementTo)
        {
            List<Parameter> parametersFrom = ConvertParameterSet(elementFrom.Parameters);
            List<Parameter> parametersTo = ConvertParameterSet(elementTo.Parameters);

            List<ElementId> parametersToIds = parametersTo.ConvertAll(x => x.Id);

            // List of parameter that not to change.
            List<string> parametersExclude = new List<string>
            {
                "Family",
                "Family and Type",
                "Image",
                "Type"
            };

            foreach (Parameter parameter in parametersFrom)
            {
                if (parametersExclude.Contains(parameter.Definition.Name) || parameter.IsReadOnly)
                    continue;

                if (parametersToIds.Contains(parameter.Id))
                {
                    Parameter parameterTo = parametersTo.ElementAt(parametersToIds.IndexOf(parameter.Id));

                    switch (parameter.StorageType)
                    {
                        case StorageType.Double:
                            double valueD = parameter.AsDouble();
                            parameterTo.Set(valueD);
                            _countPropertiesMatched++;
                            break;
                        case StorageType.ElementId:
                            ElementId valueE = parameter.AsElementId();
                            parameterTo.Set(valueE);
                            _countPropertiesMatched++;
                            break;
                        case StorageType.Integer:
                            int valueI = parameter.AsInteger();
                            parameterTo.Set(valueI);
                            _countPropertiesMatched++;
                            break;
                        case StorageType.String:
                            string valueS = parameter.AsString();
                            parameterTo.Set(valueS);
                            _countPropertiesMatched++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts parameter set to list of parameters
        /// </summary>
        /// <returns>List of parameters.</returns>
        private static List<Parameter> ConvertParameterSet(ParameterSet parametersSet)
        {
            List<Parameter> parametersList = new List<Parameter>();

            ParameterSetIterator iterator = parametersSet.ForwardIterator();
            while (iterator.MoveNext())
            {
                parametersList.Add(iterator.Current as Parameter);
            }

            return parametersList;
        }

        private string GetRunResult()
        {
            string text = (_countPropertiesMatched == 0)
                    ? "No properties set."
                    : $"{_countPropertiesMatched} properties have been matched.";

            return text;
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, _runResult);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementPropertiesMatch).Namespace + "." + nameof(ElementPropertiesMatch);
        }
    }
}