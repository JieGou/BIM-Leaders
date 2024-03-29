﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Logic
{
    public class SelectReferencePlanesModel : INotifyPropertyChanged
    {
        private UIDocument _uidoc;
        private Document _doc;
        private double _toleranceAngle;

        #region PROPERTIES

        private List<int> _selectedElements;
        public List<int> SelectedElements
        {
            get { return _selectedElements; }
            set
            {
                _selectedElements = value;
                OnPropertyChanged(nameof(SelectedElements));
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

        public SelectReferencePlanesModel(BaseModel model)
        {
            _uidoc = model.Uidoc;
            _doc = _uidoc.Document;
            _toleranceAngle = _doc.Application.AngleTolerance / 100; // 0.001 grad.
        }

        public void Run()
        {
            Error = "";

            try
            {
                // Get the line from user selection
                IList<Reference> referenceUncheckedList = _uidoc.Selection.PickObjects(ObjectType.Element, new SelectionFilterByCategory("Reference Planes"), "Select Two Perpendicular Reference Planes");

                // Checking for invalid selection.
                if (referenceUncheckedList.Count != 2)
                {
                    Error = "Wrong count of reference planes selected";
                    SelectedElements = new List<int> { 0, 0 };
                    return;
                }

                // Getting Reference planes.
                ReferencePlane reference0 = _doc.GetElement(referenceUncheckedList[0].ElementId) as ReferencePlane;
                ReferencePlane reference1 = _doc.GetElement(referenceUncheckedList[1].ElementId) as ReferencePlane;

                // Checking for perpendicular input
                if (reference0.Direction.DotProduct(reference1.Direction) > _toleranceAngle)
                {
                    Error = "Selected reference planes are not perpendicular";
                    SelectedElements = new List<int> { 0, 0 };
                    return;
                }

                SelectedElements = (Error.Length == 0)
                    ? new List<int> { reference0.Id.IntegerValue, reference1.Id.IntegerValue }
                    : new List<int> { 0, 0 };
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException oce)
            {
                Error = "Selection cancelled";
                SelectedElements = new List<int> { 0, 0 };
            }
            catch (Exception e)
            {
                Error = e.Message;
                SelectedElements = new List<int> { 0, 0 };
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