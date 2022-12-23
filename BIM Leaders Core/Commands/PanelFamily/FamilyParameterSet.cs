using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterSet : BaseCommand
    {
        public FamilyParameterSet()
        {
            _transactionName = "Set Parameter";

            _model = new FamilyParameterSetModel();
            _viewModel = new FamilyParameterSetViewModel();
            _view = new FamilyParameterSetForm();
        }

        public static string GetPath() => typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
    }
}