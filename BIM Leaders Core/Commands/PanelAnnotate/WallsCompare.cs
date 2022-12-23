using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsCompare : BaseCommand
    {
        public WallsCompare()
        {
            _transactionName = "Compare Walls";

            _model = new WallsCompareModel();
            _viewModel = new WallsCompareViewModel();
            _view = new WallsCompareForm();
        }

        public static string GetPath() => typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
    }
}