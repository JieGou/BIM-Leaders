using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        }

        private protected override async void Run(ExternalCommandData commandData)
        {
            // Model
            TagsPlanCheckM formM = new TagsPlanCheckM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            TagsPlanCheckVM formVM = new TagsPlanCheckVM(formM);

            // View
            TagsPlanCheckForm form = new TagsPlanCheckForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(TagsPlanCheck).Namespace + "." + nameof(TagsPlanCheck);
    }
}