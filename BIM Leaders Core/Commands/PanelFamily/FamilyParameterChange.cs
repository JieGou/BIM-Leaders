using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterChange : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Change Parameter";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static Document _doc;
        private static int _countParametersChanged = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _runStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    SwitchParameter();

                    trans.Commit();
                }

                _runResult = GetRunResult();

                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Change the given parameter from shared to family parameter.
        /// </summary>
        private static void SwitchParameter()
        {
            IEnumerable<FamilyParameter> parameters = _doc.FamilyManager.GetParameters().Where(x => x.IsShared);
            foreach (FamilyParameter parameter in parameters)
            {
                string parameterName = parameter.Definition.Name;
                string parameterNameTemp = parameterName + "_";
                BuiltInParameterGroup parameterGroup = parameter.Definition.ParameterGroup;
                bool parameterIsInstance = parameter.IsInstance;

                FamilyParameter parameterNew = _doc.FamilyManager.ReplaceParameter(parameter, parameterNameTemp, parameterGroup, parameterIsInstance);
                _doc.FamilyManager.RenameParameter(parameterNew, parameterName);
                
                _countParametersChanged++;
            }
            _countParametersChanged = parameters.Count();
        }

        private string GetRunResult()
        {
            string text = (_countParametersChanged == 0)
                    ? "No parameters changed."
                    : $"{_countParametersChanged} parameters changed.";

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
            return typeof(FamilyParameterChange).Namespace + "." + nameof(FamilyParameterChange);
        }
    }
}