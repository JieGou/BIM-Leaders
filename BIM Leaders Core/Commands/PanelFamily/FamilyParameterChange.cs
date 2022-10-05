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
        private static int _countParametersChanged = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                using (Transaction trans = new Transaction(doc, "Change Parameter"))
                {
                    trans.Start();

                    SwitchParameter(doc);

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
        private static void SwitchParameter(Document doc)
        {
            IEnumerable<FamilyParameter> parameters = doc.FamilyManager.GetParameters().Where(x => x.IsShared);
            foreach (FamilyParameter parameter in parameters)
            {
                string parameterName = parameter.Definition.Name;
                string parameterNameTemp = parameterName + "_";
                BuiltInParameterGroup parameterGroup = parameter.Definition.ParameterGroup;
                bool parameterIsInstance = parameter.IsInstance;

                FamilyParameter parameterNew = doc.FamilyManager.ReplaceParameter(parameter, parameterNameTemp, parameterGroup, parameterIsInstance);
                doc.FamilyManager.RenameParameter(parameterNew, parameterName);
                
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
            
            TaskDialog.Show("Parameter Change", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterChange).Namespace + "." + nameof(FamilyParameterChange);
        }
    }
}