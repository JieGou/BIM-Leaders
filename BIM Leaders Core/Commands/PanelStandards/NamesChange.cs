using System.Threading.Tasks;
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

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            NamesChangeM formM = new NamesChangeM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            NamesChangeVM formVM = new NamesChangeVM(formM);

            // View
            NamesChangeForm form = new NamesChangeForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }   

        public static string GetPath()
        {
            return typeof(NamesChange).Namespace + "." + nameof(NamesChange);
        }
    }
}