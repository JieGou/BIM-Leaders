using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            CheckerModel formM = new CheckerModel(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            CheckerViewModel formVM = new CheckerViewModel(formM);
            
            // View
            CheckerForm form = new CheckerForm() { DataContext = formVM };
            form.ShowDialog();
        }
        public static string GetPath() => typeof(Checker).Namespace + "." + nameof(Checker);
    }
}