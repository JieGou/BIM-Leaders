using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsArranged : BaseCommand
    {
        public WallsArranged()
        {
            _transactionName = "Annotate Section";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Models
            WallsArrangedM formM = new WallsArrangedM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectReferencePlanesM formSelectionM = new SelectReferencePlanesM(commandData);

            // ViewModel
            WallsArrangedVM formVM = new WallsArrangedVM(formM, formSelectionM);

            // View
            WallsArrangedForm form = new WallsArrangedForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath()
        {
            return typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
        }
    }
}