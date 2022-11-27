using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterSet : BaseCommand
    {
        private List<string> _parametersList;

        public FamilyParameterSet()
        {
            _transactionName = "Set Parameter";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult();
            _parametersList = GetParametersList(commandData);

            Run(commandData);

            if (!_result.Started)
                return Result.Cancelled;
            if (_result.Failed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            FamilyParameterSetM formM = new FamilyParameterSetM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            FamilyParameterSetVM formVM = new FamilyParameterSetVM(formM)
            {
                ParametersList = _parametersList
            };

            // View
            FamilyParameterSetForm form = new FamilyParameterSetForm() { DataContext = formVM };
            form.ShowDialog();
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

        public static string GetPath() => typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
    }
}