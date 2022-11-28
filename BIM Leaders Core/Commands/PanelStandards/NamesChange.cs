using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class NamesChange : BaseCommand
    {
        public NamesChange()
        {
            _transactionName = "Change Names";

            _model = new NamesChangeModel();
            _viewModel = new NamesChangeViewModel();
            _view = new NamesChangeForm();
        } 

        public static string GetPath() => typeof(NamesChange).Namespace + "." + nameof(NamesChange);
    }
}