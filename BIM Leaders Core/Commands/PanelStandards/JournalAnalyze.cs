using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class JournalAnalyze : BaseCommand
    {
        public JournalAnalyze()
        {
            _transactionName = "Analyze Journal";

            _model = new JournalAnalyzeModel();
            _viewModel = new JournalAnalyzeViewModel();
            _view = new JournalAnalyzeForm();
        }

        public static string GetPath() => typeof(JournalAnalyze).Namespace + "." + nameof(JournalAnalyze);
    }
}