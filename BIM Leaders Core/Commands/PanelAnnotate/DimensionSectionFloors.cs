using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloors : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Annotate Section";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CheckIfSectionIsSplit(commandData);

            Run(commandData);

            return Result.Succeeded;
        }

        private void CheckIfSectionIsSplit(ExternalCommandData commandData)
        {
#if !VERSION2020
            ViewSection view = commandData.Application.ActiveUIDocument.Document.ActiveView as ViewSection;
            if (view.IsSplitSection())
                ShowResult("Current view is a split section. This may cause issues when finding geometry intersections.");
#endif
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionSectionFloorsM formM = new DimensionSectionFloorsM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionSectionFloorsVM formVM = new DimensionSectionFloorsVM(formM, formSelectionM);

            // View
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm(formVM) { DataContext = formVM };
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
            // Return constructed namespace path
            return typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
        }
    }
}