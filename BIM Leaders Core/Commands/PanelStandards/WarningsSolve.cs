using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WarningsSolve : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Solve Warnings";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            WarningsSolveM formM = new WarningsSolveM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WarningsSolveVM formVM = new WarningsSolveVM(formM);

            // View
            WarningsSolveForm form = new WarningsSolveForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ResultVM formVM = new ResultVM(TRANSACTION_NAME, _runResult);

            // View
            ResultForm form = new ResultForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}