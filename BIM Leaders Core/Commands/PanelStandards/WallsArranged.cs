using System.Threading.Tasks;
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

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Models
            WallsArrangedM formM = new WallsArrangedM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectReferencePlanesM formSelectionM = new SelectReferencePlanesM(commandData);

            // ViewModel
            WallsArrangedVM formVM = new WallsArrangedVM(formM, formSelectionM);

            // View
            WallsArrangedForm form = new WallsArrangedForm(formVM) { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

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