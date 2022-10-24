using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class DwgNameDeleteM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private string _dwgName;
        private int _countDwgDeleted;

        private const string TRANSACTION_NAME = "Delete DWG by Name";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private int _dwgListSelected;
        public int DwgListSelected
        {
            get { return _dwgListSelected; }
            set
            {
                _dwgListSelected = value;
                OnPropertyChanged(nameof(DwgListSelected));
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

        #endregion

        public DwgNameDeleteM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TRANSACTION_NAME;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                DeleteDwg();
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }

            ShowResult();
        }

        #endregion

        #region METHODS

        private void DeleteDwg()
        {
            ElementId dwgId = new ElementId(DwgListSelected);
            string _dwgName = _doc?.GetElement(dwgId).Category.Name;

            // Get all Imports with name same as input from a form
            ICollection<ElementId> dwgDelete = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Where(x => x.Category.Name == _dwgName)
                .ToList()
                .ConvertAll(x => x.Id)
                .ToList();

            _doc.Delete(dwgDelete);

            _countDwgDeleted = dwgDelete.Count;
        }

        private void ShowResult()
        {
            if (RunResult.Length == 0)
            {
                RunResult = (_countDwgDeleted == 0)
                    ? "No DWG deleted"
                    : $"{_countDwgDeleted} DWG named {_dwgName} deleted";
            }

            TaskDialog.Show(TRANSACTION_NAME, RunResult);
        }

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