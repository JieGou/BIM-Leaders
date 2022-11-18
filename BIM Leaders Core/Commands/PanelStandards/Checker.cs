using System.Threading.Tasks;
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

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            CheckerM formM = new CheckerM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            CheckerVM formVM = new CheckerVM(formM);

            // View
            CheckerForm form = new CheckerForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;
            _runReport = formM.RunReport;

            ShowResult();
        }

        public static string GetPath() => typeof(Checker).Namespace + "." + nameof(Checker);
    }
}