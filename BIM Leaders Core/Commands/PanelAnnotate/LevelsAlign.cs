using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class LevelsAlign : BaseCommand
    {
        public LevelsAlign()
        {
            _transactionName = "Align Levels";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            LevelsAlignM formM = new LevelsAlignM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            LevelsAlignVM formVM = new LevelsAlignVM(formM);

            // View
            LevelsAlignForm form = new LevelsAlignForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath() => typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
    }
}