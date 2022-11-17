using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloors : BaseCommand
    {
        public DimensionSectionFloors()
        {
            _transactionName = "Annotate Section";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CheckIfSectionIsSplit(commandData);

            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private void CheckIfSectionIsSplit(ExternalCommandData commandData)
        {
#if !VERSION2020
            ViewSection view = commandData.Application.ActiveUIDocument.Document.ActiveView as ViewSection;
            if (view.IsSplitSection())
            {
                _runResult = "Current view is a split section. This may cause issues when finding geometry intersections.";
                ShowResult();
            }
#endif
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionSectionFloorsM formM = new DimensionSectionFloorsM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionSectionFloorsVM formVM = new DimensionSectionFloorsVM(formM, formSelectionM);

            // View
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath()
        {
            return typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
        }
    }
}