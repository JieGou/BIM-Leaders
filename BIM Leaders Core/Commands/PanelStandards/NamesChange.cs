﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Threading.Tasks;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class NamesChange : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Change Names";

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
            // Model
            NamesChangeM formM = new NamesChangeM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            NamesChangeVM formVM = new NamesChangeVM(formM);

            // View
            NamesChangeForm form = new NamesChangeForm() { DataContext = formVM };
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
            // Return constructed namespace path
            return typeof(NamesChange).Namespace + "." + nameof(NamesChange);
        }
    }
}