using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterSet : IExternalCommand
    {
        private Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            // Model
            FamilyParameterSetM formM = new FamilyParameterSetM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            FamilyParameterSetVM formVM = new FamilyParameterSetVM(formM);
            formVM.ParametersList = GetParametersList();

            // View
            FamilyParameterSetForm form = new FamilyParameterSetForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        private static List<string> GetParametersList()
        {
            // Get unique parameters
            IList<FamilyParameter> parametersNamesAll = _doc.FamilyManager.GetParameters();
            List<FamilyParameter> parameters = new List<FamilyParameter>();
            List<string> parametersNames = new List<string>();
            foreach (FamilyParameter i in parametersNamesAll)
            {
                string parameterName = i.Definition.Name;
                if (!parametersNames.Contains(parameterName))
                {
                    parameters.Add(i);
                    parametersNames.Add(parameterName);
                }
            }
            return parametersNames;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
        }
    }
}