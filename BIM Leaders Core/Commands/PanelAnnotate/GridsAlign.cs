using System.Threading.Tasks;
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

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            GridsAlignM formM = new GridsAlignM(commandData, _transactionName);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            GridsAlignVM formVM = new GridsAlignVM(formM);

            // View
            GridsAlignForm form = new GridsAlignForm() { DataContext = formVM };
            form.ShowDialog();

            while(!formVM.Closed)
                await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        public static string GetPath() => typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
    }
}