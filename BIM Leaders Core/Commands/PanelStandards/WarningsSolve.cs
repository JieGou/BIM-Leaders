using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using BIM_Leaders_Logic;
using System.Threading.Tasks;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WarningsSolve : IExternalCommand
    {
        private static Document _doc;

        private const string TRANSACTION_NAME = "Solve Warnings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            WarningsSolveM formM = new WarningsSolveM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WarningsSolveVM formVM = new WarningsSolveVM(formM);

            // View
            WarningsSolveForm form = new WarningsSolveForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            if (resultText == null)
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}