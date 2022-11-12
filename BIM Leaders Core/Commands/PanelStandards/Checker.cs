using System.Data;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Windows.Media;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Checker : IExternalCommand
    {
        private static DataSet _reportDataSet;

        private const string TRANSACTION_NAME = "Check";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Task<DataSet> runTask = new Task<DataSet>(() => Run(commandData));
            Task resultTask = runTask.ContinueWith(t => ShowResult());

            runTask.Start();
            resultTask.Wait();

            return Result.Succeeded;
        }

        private DataSet Run(ExternalCommandData commandData)
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

            //await Task.Delay(1000);

            _reportDataSet = formM.ReportDataSet;
            return formM.ReportDataSet;

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            if (resultText == null)
                return;
            if (resultText.Length > 0)
            {
                // ViewModel
                ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

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