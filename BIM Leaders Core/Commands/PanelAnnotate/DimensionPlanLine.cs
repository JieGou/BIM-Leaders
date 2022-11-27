using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionPlanLine : BaseCommand
    {
        public DimensionPlanLine()
        {
            _transactionName = "Dimension Plan Walls";

            _model = new DimensionPlanLineModel();
            _viewModel = new DimensionPlanLineViewModel();
            _viewModel.SelectLineModel = new ;
            _view = new DimensionPlanLineForm();
        }

        public static string GetPath() => typeof(DimensionPlanLine).Namespace + "." + nameof(DimensionPlanLine);
    }
}