using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            NamesChangeModel formM = new NamesChangeM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            NamesChangeViewModel formVM = new NamesChangeViewModel(formM);

            // View
            NamesChangeForm form = new NamesChangeForm() { DataContext = formVM };
            form.ShowDialog();
        }   

        public static string GetPath() => typeof(NamesChange).Namespace + "." + nameof(NamesChange);
    }
}