using System.Data;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Checker : IExternalCommand
    {
        private static DataSet _reportDataSet;
        private bool _runFailed;
        private string _runResult;

        private const string TRANSACTION_NAME = "Check";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData).Wait();

            CheckerM m = Run(commandData).Result;

            _runFailed = m.RunFailed;
            _runResult = m.RunResult;

            ShowResult();

            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private async Task<CheckerM> Run(ExternalCommandData commandData)
        {
            // Model
            CheckerM formM = new CheckerM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            CheckerVM formVM = new CheckerVM(formM);

            // View
            CheckerForm form = new CheckerForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            return formM;

            
        }

        private void ShowResult()
        {
            if (_runResult.Length > 0)
            {
                // ViewModel
                ReportVM formVM = new ReportVM(TRANSACTION_NAME, _runResult);

                // View
                ReportForm form = new ReportForm() { DataContext = formVM };
                form.ShowDialog();
            }
            else
            {
                // ViewModel
                CheckerReportVM formReportVM = new CheckerReportVM(_reportDataSet);

                // View
                CheckerReportForm formReport = new CheckerReportForm() { DataContext = formReportVM };
                formReport.ShowDialog();
            }
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Checker).Namespace + "." + nameof(Checker);
        }
    }
}