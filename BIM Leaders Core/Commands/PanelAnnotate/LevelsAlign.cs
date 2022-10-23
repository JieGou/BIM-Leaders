using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class LevelsAlign : IExternalCommand
    {
        private static Document _doc;
        private static LevelsAlignVM _inputData;
        private static int _countLevelsAligned;

        private const string TRANSACTION_NAME = "Align Levels";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Model
            LevelsAlignM formM = new LevelsAlignM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            LevelsAlignVM formVM = new LevelsAlignVM(formM);

            // View
            LevelsAlignForm form = new LevelsAlignForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
        }
    }
}
