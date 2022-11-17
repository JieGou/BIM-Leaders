﻿using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArranged : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Annotate Section";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Models
            WallsArrangedM formM = new WallsArrangedM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectReferencePlanesM formSelectionM = new SelectReferencePlanesM(commandData);

            // ViewModel
            WallsArrangedVM formVM = new WallsArrangedVM(formM, formSelectionM);

            // View
            WallsArrangedForm form = new WallsArrangedForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
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
            // Return constructed namespace path.
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}