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

            _model = new DimensionSectionFloorsModel();
            _viewModel = new DimensionPlanLineViewModel();
            _view = new DimensionPlanLineForm();
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult() { Started = true };

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

        public static string GetPath() => typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
    }
}