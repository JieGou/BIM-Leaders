using System;
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
            //form.ShowDialog();
            
            Func<Task> showAsync = async () =>
            {
                await Task.Yield();
                form.ShowDialog();
            };

            var dialogTask = showAsync();
            await Task.Yield();
            await dialogTask;

            await Task.Run(() => showAsync);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;
            _runReport = formM.RunReport;

            ShowResult();
        }
        

        private protected override async Task<string> RunAsync(ExternalCommandData commandData)
        {
            // Model
            CheckerM formM = new CheckerM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            CheckerVM formVM = new CheckerVM(formM);

            // View
            CheckerForm form = new CheckerForm() { DataContext = formVM };
            await Task.Run(() => form.ShowDialog());
            /*
            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;
            _runReport = formM.RunReport;

            ShowResult();
            */
            return formM.RunResult; 
        }

        public static string GetPath() => typeof(Checker).Namespace + "." + nameof(Checker);
    }
}