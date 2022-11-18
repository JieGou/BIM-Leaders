using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WarningsSolve : BaseCommand
    {
        public WarningsSolve()
        {
            _transactionName = "Solve Warnings";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            WarningsSolveM formM = new WarningsSolveM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WarningsSolveVM formVM = new WarningsSolveVM(formM);

            // View
            WarningsSolveForm form = new WarningsSolveForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath()=> typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
    }
}