using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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

            _model = new LevelsAlignModel();
            _viewModel = new LevelsAlignViewModel();
            _view = new LevelsAlignForm();
        }

        public static string GetPath() => typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
    }
}