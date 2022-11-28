using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class GridsAlign : BaseCommand
    {
        public GridsAlign()
        {
            _transactionName = "Align Grids";
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            GridsAlignModel formM = new GridsAlignModel(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            GridsAlignViewModel formVM = new GridsAlignViewModel(formM);

            // View
            GridsAlignForm form = new GridsAlignForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
    }
}