using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class TagsPlanCheck : BaseCommand
    {
        public TagsPlanCheck()
        {
            _transactionName = "Tags Plan Check";

            _model = new TagsPlanCheckModel();
            _viewModel = new TagsPlanCheckViewModel();
            _view = new TagsPlanCheckForm();
        }

        public static string GetPath() => typeof(TagsPlanCheck).Namespace + "." + nameof(TagsPlanCheck);
    }
}