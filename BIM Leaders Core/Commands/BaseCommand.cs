using System.Data;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using Autodesk.Revit.UI.Selection;
using System;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public abstract class BaseCommand : IExternalCommand
    {
        private Type _modelType;
        private Type _viewModelType;
        private Type _viewType;

        private static DataSet _reportDataSet;
        private bool _runFailed;
        private string _runResult;

        private const string TRANSACTION_NAME = "Check";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //_modelType = typeof(CheckerM);
            //_viewModelType = typeof(CheckerVM);
            //_viewType = typeof(CheckerForm);

            GetInput(commandData);

            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private async void GetInput(ExternalCommandData commandData)
        {
            // Model
            BaseModel model = Activator.CreateInstance(_modelType) as BaseModel;
            ExternalEvent externalEvent = ExternalEvent.Create(model);
            model.ExternalEvent = externalEvent;
            model.CommandData = commandData;
            model.TransactionName = TRANSACTION_NAME;

            // ViewModel
            BaseViewModel formVM = new BaseViewModel(model);

            // View
            BaseView form = new BaseView() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runFailed = model.RunFailed;
            _runResult = model.RunResult;

            ShowResult();
        }

        private void ShowResult()
        {
            if (_runResult.Length > 0)
            {
                // ViewModel
                ReportVM formVM = new ReportVM(TRANSACTION_NAME, _runResult);

                // View
                ReportForm form = new ReportForm() { DataContext = formVM };
                form.ShowDialog();
            }
            else
            {
                // ViewModel
                BaseReportViewModel formReportVM = new BaseReportViewModel(_reportDataSet);

                // View
                BaseReportView formReport = new BaseReportView() { DataContext = formReportVM };
                formReport.ShowDialog();
            }
        }

        /// <summary>
        /// Return constructed namespace path
        /// </summary>
        public virtual string GetPath()
        {
            return typeof(BaseCommand).Namespace + "." + nameof(BaseCommand);
        }
    }
}