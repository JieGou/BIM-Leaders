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

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            DimensionsPlanCheckModel formM = new DimensionsPlanCheckM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionsPlanCheckViewModel formVM = new DimensionsPlanCheckVM(formM);

            // View
            DimensionsPlanCheckForm form = new DimensionsPlanCheckForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
    }
}