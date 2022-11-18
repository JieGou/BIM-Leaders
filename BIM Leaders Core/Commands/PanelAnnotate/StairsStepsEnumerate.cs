﻿using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerate : BaseCommand
    {
        public StairsStepsEnumerate()
        {
            _transactionName = "Number Steps";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            StairsStepsEnumerateM formM = new StairsStepsEnumerateM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            StairsStepsEnumerateVM formVM = new StairsStepsEnumerateVM(formM);

            // View
            StairsStepsEnumerateForm form = new StairsStepsEnumerateForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath() => typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
    }
}