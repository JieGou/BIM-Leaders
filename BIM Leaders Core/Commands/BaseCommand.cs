using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
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
    }
}