using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Data;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Purge : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Purge";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;
        private DataSet _runReport;

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
            PurgeM formM = new PurgeM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            PurgeVM formVM = new PurgeVM(formM);

            // View
            PurgeForm form = new PurgeForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;
            _runReport = formM.RunReport;

            ShowResult();
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (!string.IsNullOrEmpty(_runResult))
            {
                // ViewModel
                ResultVM formVM = new ResultVM(TRANSACTION_NAME, _runResult);

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

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Purge).Namespace + "." + nameof(Purge);
        }
    }
}