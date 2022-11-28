using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsParallel : BaseCommand
    {
        public WallsParallel()
        {
            _transactionName = "Walls Parallel Check";

            _model = new WallsParallelModel();
            _viewModel = new WallsParallelViewModel();
            _view = new WallsParallelForm();
        }

        public static string GetPath() => typeof(WallsParallel).Namespace + "." + nameof(WallsParallel);
    }
}