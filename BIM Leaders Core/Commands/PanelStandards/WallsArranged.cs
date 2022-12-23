using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArranged : BaseCommand
    {
        public WallsArranged()
        {
            _transactionName = "Annotate Section";

            _model = new WallsArrangedModel();
            _viewModel = new WallsArrangedViewModel();
            _view = new WallsArrangedForm();
        }

        public static string GetPath() => typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
    }
}