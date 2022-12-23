using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class GridsAlign : BaseCommand
    {
        public GridsAlign()
        {
            _transactionName = "Align Grids";

            _model = new GridsAlignModel();
            _viewModel = new GridsAlignViewModel();
            _view = new GridsAlignForm();
        }

        public static string GetPath() => typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
    }
}