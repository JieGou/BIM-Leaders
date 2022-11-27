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

            _model = new PurgeM();
            _viewModel = new PurgeVM();
            _view = new PurgeForm();
        }
        /*
        private protected override void RunOld(ExternalCommandData commandData)
        {
            // Model
            PurgeM formM = new PurgeM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            PurgeVM formVM = new PurgeVM(formM);

            // View
            PurgeForm form = new PurgeForm() { DataContext = formVM };
            form.ShowDialog();
        }
        */
        public static string GetPath() => typeof(Purge).Namespace + "." + nameof(Purge);
    }
}