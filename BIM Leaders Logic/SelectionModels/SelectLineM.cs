using System;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Logic
{
    public class SelectLineM : INotifyPropertyChanged
    {
        private UIDocument _uidoc;
        private Document _doc;

        #region PROPERTIES

        private bool _allowOnlyVertical;
        public bool AllowOnlyVertical
        {
            get { return _allowOnlyVertical; }
            set { _allowOnlyVertical = value; }
        }

        private int _selectedElement;
        public int SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                _selectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        private string _error;
        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged(nameof(Error));
            }
        }

        #endregion

        public SelectLineM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            Error = "";

            // Get the line from user selection
            Reference referenceLine = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Lines"), "Select Line");
            DetailCurve detailCurve = _doc.GetElement(referenceLine) as DetailCurve;
            if (detailCurve == null)
                Error = "Wrong selection";

            if (detailCurve.GeometryCurve is Line line)
            {
                if (AllowOnlyVertical)
                {
                    double direction = line.Direction.Z;
                    if (direction != 1 && direction != -1)
                        Error = "Selected line is not vertical";
                }
            }   
            else
                Error = "Selected line is not straight";

            SelectedElement = (Error.Length == 0)
                ? detailCurve.Id.IntegerValue
                : 0;
        }

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