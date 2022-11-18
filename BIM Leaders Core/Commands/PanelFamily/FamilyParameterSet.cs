﻿using System.Collections.Generic;
using System.Threading.Tasks;
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
            _parametersList = GetParametersList(commandData);

            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            FamilyParameterSetM formM = new FamilyParameterSetM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            FamilyParameterSetVM formVM = new FamilyParameterSetVM(formM);
            formVM.ParametersList = _parametersList;

            // View
            FamilyParameterSetForm form = new FamilyParameterSetForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
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