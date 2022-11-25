using System;
using System.Data;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public abstract class BaseModel : INotifyPropertyChanged, IExternalEventHandler
    {
        private protected UIDocument _uidoc;
        private protected Document _doc;
        private protected RunResult _result;
        private protected Action<string, RunResult> _showResult;

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private string _transactionName;
        public string TransactionName
        {
            get { return _transactionName; }
            set
            {
                _transactionName = value;
                OnPropertyChanged(nameof(TransactionName));
            }
        }

        #endregion

        public BaseModel(ExternalCommandData commandData, string transactionName, Action<string, RunResult> showResultAction)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
            _result = new RunResult();
            _showResult = showResultAction;

            TransactionName = transactionName;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TransactionName;
        }

        public virtual void Execute(UIApplication app)
        {
            _result.Started = true;

            try
            {
                TryExecute();
            }
            catch (Exception e)
            {
                _result.Failed = true;
                _result.Result = ExceptionUtils.GetMessage(e);
            }
            finally
            {
                _showResult(TransactionName, _result);
            }
        }

        private protected abstract void TryExecute();

        #endregion

        #region METHODS

        private protected virtual string GetRunResult() => "";
        private protected virtual DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages) => null;

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}