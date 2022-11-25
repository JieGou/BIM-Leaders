using System.Data;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using Autodesk.Revit.UI.Selection;
using System;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public abstract class BaseCommand : IExternalCommand
    {
        private protected string _transactionName;
        private protected bool _runStarted;
        private protected bool _runFailed;
        private protected string _runResult;
        private protected DataSet _runReport;

        public virtual Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private protected abstract void Run(ExternalCommandData commandData);

        //private protected virtual Task<DataSet> RunAsync(ExternalCommandData commandData) { return null; } //

        private protected void ShowResult()
        {
            if (!_runStarted)
                return;
            if (!string.IsNullOrEmpty(_runResult))
            {
                // ViewModel
                ResultVM formVM = new ResultVM(_transactionName, _runResult);

                // View
                ResultForm form = new ResultForm() { DataContext = formVM };
                form.ShowDialog();

                return;
            }
            if (_runReport != null)
            {
                // ViewModel
                ReportVM formReportVM = new ReportVM(_runReport);

                // View
                ReportForm formReport = new ReportForm() { DataContext = formReportVM };
                formReport.ShowDialog();
            }
        }

        private protected void ShowResult(string transactionName, RunResult runResult)
        {
            if (!runResult.Started)
                return;
            if (!string.IsNullOrEmpty(runResult.Result))
            {
                // ViewModel
                ResultVM formVM = new ResultVM(transactionName, runResult.Result);

                // View
                ResultForm form = new ResultForm() { DataContext = formVM };
                form.ShowDialog();

                return;
            }
            if (runResult.Report != null)
            {
                // ViewModel
                ReportVM formReportVM = new ReportVM(runResult.Report);

                // View
                ReportForm formReport = new ReportForm() { DataContext = formReportVM };
                formReport.ShowDialog();
            }
        }

        /// <summary>
        /// Return constructed namespace path
        /// </summary>
        public virtual string GetPath()
        {
            return typeof(BaseCommand).Namespace + "." + nameof(BaseCommand);
        }
    }
}