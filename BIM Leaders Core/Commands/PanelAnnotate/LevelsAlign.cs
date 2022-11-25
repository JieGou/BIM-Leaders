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

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            LevelsAlignM formM = new LevelsAlignM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            LevelsAlignVM formVM = new LevelsAlignVM(formM);

            // View
            LevelsAlignForm form = new LevelsAlignForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
    }
}