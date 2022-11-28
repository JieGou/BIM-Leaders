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

            //_viewModel.SelectReferencePlanesModel;
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            WallsArrangedModel formM = new WallsArrangedM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectReferencePlanesM formSelectionM = new SelectReferencePlanesM(commandData);

            // ViewModel
            WallsArrangedViewModel formVM = new WallsArrangedViewModel(formM, formSelectionM);

            // View
            WallsArrangedForm form = new WallsArrangedForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(WallsArranged).Namespace + "." + nameof(WallsArranged);
    }
}