using System.Collections.Generic;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterSet : IExternalCommand
    {
        private List<string> _parametersList;

        private const string TRANSACTION_NAME = "Set Parameter";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _parametersList = GetParametersList(commandData);

            Run(commandData);

            return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            FamilyParameterSetM formM = new FamilyParameterSetM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            FamilyParameterSetVM formVM = new FamilyParameterSetVM(formM);
            formVM.ParametersList = _parametersList;

            // View
            FamilyParameterSetForm form = new FamilyParameterSetForm(formVM) { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private List<string> GetParametersList(ExternalCommandData commandData)
        {
            // Get unique parameters
            IList<FamilyParameter> parametersNamesAll = commandData.Application.ActiveUIDocument.Document.FamilyManager.GetParameters();
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

        private void ShowResult(string resultText)
        {
            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
        }
    }
}