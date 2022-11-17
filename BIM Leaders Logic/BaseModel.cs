using System;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Data;
using System.Collections.Generic;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public abstract class BaseModel : INotifyPropertyChanged, IExternalEventHandler
    {
        private protected UIDocument _uidoc;
        private protected Document _doc;

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

        private bool _runStarted;
        public bool RunStarted
        {
            get { return _runStarted; }
            set
            {
                _runStarted = value;
                OnPropertyChanged(nameof(RunStarted));
            }
        }

        private bool _runFailed;
        public bool RunFailed
        {
            get { return _runFailed; }
            set
            {
                _runFailed = value;
                OnPropertyChanged(nameof(RunFailed));
            }
        }

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            {
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        private DataSet _runReport;
        public DataSet RunReport
        {
            get { return _runReport; }
            set
            {
                _runReport = value;
                OnPropertyChanged(nameof(RunReport));
            }
        }

        #endregion

        public BaseModel(ExternalCommandData commandData, string transactionName)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

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

        public abstract void Execute(UIApplication app);

        #endregion

        #region METHODS

        private protected abstract string GetRunResult();
        private protected abstract DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages);

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