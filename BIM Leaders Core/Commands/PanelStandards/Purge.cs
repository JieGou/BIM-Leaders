using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            PurgeM formM = new PurgeM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            PurgeVM formVM = new PurgeVM(formM);

            // View
            PurgeForm form = new PurgeForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;
            _runReport = formM.RunReport;

            ShowResult();
        }

        public static string GetPath()
        {
            return typeof(Purge).Namespace + "." + nameof(Purge);
        }
    }
}