using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionsPlanCheck : BaseCommand
    {
        public DimensionsPlanCheck()
        {
            _transactionName = "Create Filter for non-dimensioned Walls";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            DimensionsPlanCheckM formM = new DimensionsPlanCheckM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionsPlanCheckVM formVM = new DimensionsPlanCheckVM(formM);

            // View
            DimensionsPlanCheckForm form = new DimensionsPlanCheckForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath()
        {
            return typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
        }
    }
}