using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class DwgViewFoundM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;

        private const string TRANSACTION_NAME = "Imports";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private string _selectedDwg;
        public string SelectedDwg
        {
            get { return _selectedDwg; }
            set
            {
                _selectedDwg = value;
                OnPropertyChanged(nameof(SelectedDwg));
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

        public DwgViewFoundM(ExternalCommandData commandData)
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
                // Get Imports
                IEnumerable<ImportInstance> imports = new FilteredElementCollector(_doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>();

                int dwgId = 0;
                if (!Int32.TryParse(SelectedDwg, out dwgId))
                {
                    RunResult = "Error getting a DWG from the selected item.";
                    TaskDialog.Show(TRANSACTION_NAME, RunResult);
                    return;
                }

                Element dwg = _doc.GetElement(new ElementId(dwgId));
                List<ElementId> selectionSet = new List<ElementId>() { new ElementId(dwgId) };

                if (dwg.ViewSpecific)
                {
                    View view = _doc.GetElement(dwg.OwnerViewId) as View;
                    _uidoc.ActiveView = view;
                }

                _uidoc.Selection.SetElementIds(selectionSet);
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }

            //ShowResult();
        }

        #endregion

        #region METHODS



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
