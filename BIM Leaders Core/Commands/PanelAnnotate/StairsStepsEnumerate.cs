using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerate : BaseCommand
    {
        public StairsStepsEnumerate()
        {
            _transactionName = "Number Steps";

            _model = new StairsStepsEnumerateModel();
            _viewModel = new StairsStepsEnumerateViewModel();
            _view = new StairsStepsEnumerateForm();
        }

        public static string GetPath() => typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
    }
}