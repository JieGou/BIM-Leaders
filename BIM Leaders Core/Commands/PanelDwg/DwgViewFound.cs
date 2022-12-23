using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.ReadOnly)]
    public class DwgViewFound : BaseCommand
    {
        public DwgViewFound()
        {
            _transactionName = "Imports";

            _model = new DwgViewFoundModel();
            _viewModel = new DwgViewFoundViewModel();
            _view = new DwgViewFoundForm();
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult() { Started = true };

            if (!CheckIfDocumentContainsDwg(commandData))
            {
                _result.Result = "Document has no DWG.";
                ShowResult(_result);
                return Result.Succeeded;
            }

            Run(commandData);

            if (!_result.Started)
                return Result.Cancelled;
            if (_result.Failed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private bool CheckIfDocumentContainsDwg(ExternalCommandData commandData)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int count = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .ToElementIds()
                .Count;

            return count > 0;
        }

        public static string GetPath() => typeof(DwgViewFound).Namespace + "." + nameof(DwgViewFound);
    }
}