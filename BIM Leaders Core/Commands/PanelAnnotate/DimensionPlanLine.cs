using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionPlanLine : BaseCommand
    {
        public DimensionPlanLine()
        {
            _transactionName = "Dimension Plan Walls";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionPlanLineM formM = new DimensionPlanLineM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionPlanLineVM formVM = new DimensionPlanLineVM(formM, formSelectionM);

            // View
            DimensionPlanLineForm form = new DimensionPlanLineForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath() => typeof(DimensionPlanLine).Namespace + "." + nameof(DimensionPlanLine);
    }
}