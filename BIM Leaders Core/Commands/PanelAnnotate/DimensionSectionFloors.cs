using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

            if (!_result.Started)
                return Result.Cancelled;
            if (_result.Failed)
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
                _result.Result = "Current view is a split section. This may cause issues when finding geometry intersections.";
                ShowResult(_result);
            }
#endif
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionSectionFloorsM formM = new DimensionSectionFloorsM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionSectionFloorsVM formVM = new DimensionSectionFloorsVM(formM, formSelectionM);

            // View
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
    }
}