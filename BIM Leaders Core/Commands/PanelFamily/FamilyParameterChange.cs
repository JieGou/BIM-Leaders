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
        private static Document _doc;
        private static int _countParametersChanged = 0;

        private const string TRANSACTION_NAME = "Change Parameter";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    SwitchParameter();

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

        /// <summary>
        /// Chaange the given parameter from shared to family parameter.
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

        private static void ShowResult()
        {
            // Show result
            string text = (_countParametersChanged == 0)
                ? "No parameters changed."
                : $"{_countParametersChanged} parameters changed.";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterChange).Namespace + "." + nameof(FamilyParameterChange);
        }
    }
}