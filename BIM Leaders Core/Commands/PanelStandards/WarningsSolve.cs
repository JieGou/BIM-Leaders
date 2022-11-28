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
            WarningsSolveModel formM = new WarningsSolveM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WarningsSolveViewModel formVM = new WarningsSolveViewModel(formM);

            // View
            WarningsSolveForm form = new WarningsSolveForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()=> typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
    }
}