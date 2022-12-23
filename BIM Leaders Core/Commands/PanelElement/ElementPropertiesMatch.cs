using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ElementPropertiesMatch : BaseCommand
    {
        public ElementPropertiesMatch()
        {
            _transactionName = "Match instance properties";

            _model = new ElementPropertiesMatchModel();
            _viewModel = null;
            _view = null;
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            _model.SetCommandData(commandData);
            _model.TransactionName = _transactionName;
            _model.Result = _result;
            _model.ShowResult = new System.Action<RunResult>(ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(_model);
            _model.ExternalEvent = externalEvent;

            _model.Run();
        }

        public static string GetPath() => typeof(ElementPropertiesMatch).Namespace + "." + nameof(ElementPropertiesMatch);
    }
}