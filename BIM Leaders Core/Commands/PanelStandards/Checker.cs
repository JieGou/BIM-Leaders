using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Checker : BaseCommand
    {
        public Checker()
        {
            _transactionName = "Check";

            _model = new CheckerModel();
            _viewModel = new CheckerViewModel();
            _view = new CheckerForm();
        }

        public static string GetPath() => typeof(Checker).Namespace + "." + nameof(Checker);
    }
}