using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerate : BaseCommand
    {
        public StairsStepsEnumerate()
        {
            _transactionName = "Number Steps";
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            StairsStepsEnumerateModel formM = new StairsStepsEnumerateModel(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            StairsStepsEnumerateViewModel formVM = new StairsStepsEnumerateViewModel(formM);

            // View
            StairsStepsEnumerateForm form = new StairsStepsEnumerateForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
    }
}