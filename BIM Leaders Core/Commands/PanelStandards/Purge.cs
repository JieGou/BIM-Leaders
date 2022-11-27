using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class Purge : BaseCommand
    {
        public Purge()
        {
            _transactionName = "Purge";

            _model = new PurgeModel();
            _viewModel = new PurgeViewModel();
            _view = new PurgeForm();
        }

        public static string GetPath() => typeof(Purge).Namespace + "." + nameof(Purge);
    }
}