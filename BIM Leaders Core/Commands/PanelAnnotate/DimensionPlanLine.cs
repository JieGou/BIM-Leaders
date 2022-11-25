using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionPlanLineM formM = new DimensionPlanLineM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionPlanLineVM formVM = new DimensionPlanLineVM(formM, formSelectionM);

            // View
            DimensionPlanLineForm form = new DimensionPlanLineForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DimensionPlanLine).Namespace + "." + nameof(DimensionPlanLine);
    }
}