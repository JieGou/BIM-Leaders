using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Logic
{
    public class SelectReferencePlaneModel : INotifyPropertyChanged
    {
        private UIDocument _uidoc;
        private Document _doc;

        #region PROPERTIES

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

        public SelectReferencePlaneModel(BaseModel model)
        {
            _uidoc = model.Uidoc;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            Error = "";

            try
            {
                // Get the line from user selection
                Reference referenceUnchecked = _uidoc.Selection.PickObject(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Reference Plane");

                // Getting Reference plane.
                ReferencePlane reference = _doc.GetElement(referenceUnchecked.ElementId) as ReferencePlane;

                if (reference == null)
                    Error = "Wrong selection";

                SelectedElement = (Error.Length == 0)
                    ? reference.Id.IntegerValue
                    : 0;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException oce)
            {
                Error = "Selection cancelled";
                SelectedElement = 0;
            }
            catch (Exception e)
            {
                Error = e.Message;
                SelectedElement = 0;
            }
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