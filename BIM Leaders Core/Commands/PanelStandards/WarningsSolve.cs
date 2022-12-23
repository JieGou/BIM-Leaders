using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WarningsSolve : BaseCommand
    {
        public WarningsSolve()
        {
            _transactionName = "Solve Warnings";

            _model = new WarningsSolveModel();
            _viewModel = new WarningsSolveViewModel();
            _view = new WarningsSolveForm();
        }

        public static string GetPath()=> typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
    }
}