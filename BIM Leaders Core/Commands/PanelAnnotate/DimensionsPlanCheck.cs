using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionsPlanCheck : BaseCommand
    {
        public DimensionsPlanCheck()
        {
            _transactionName = "Create Filter for non-dimensioned Walls";

            _model = new DimensionsPlanCheckModel();
            _viewModel = new DimensionsPlanCheckViewModel();
            _view = new DimensionsPlanCheckForm();
        }

        public static string GetPath() => typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
    }
}