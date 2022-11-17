using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterChange : BaseCommand
    {
        private static Document _doc;
        private static int _countParametersChanged = 0;

        public FamilyParameterChange()
        {
            _transactionName = "Change Parameter";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _runStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, _transactionName))
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

        private protected override async void Run(ExternalCommandData commandData) { return; }

        public static string GetPath()
        {
            return typeof(FamilyParameterChange).Namespace + "." + nameof(FamilyParameterChange);
        }
    }
}