﻿using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using BIM_Leaders_Logic;
using System.Collections.Generic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WallsArranged"
    /// </summary>
    public class WallsArrangedVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const double _distanceToleranceMinValue = 0.000000000001;
        private const double _distanceToleranceMaxValue = 0.1;
        private string _distanceToleranceMinValueFormat = "0.000000000000";
        private string _distanceToleranceMaxValueFormat = "0.0";

        #region PROPERTIES

        private WallsArrangedM _model;
        public WallsArrangedM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SelectReferencePlanesM _selectReferencePlanesModel;
        public SelectReferencePlanesM SelectReferencePlanesModel
        {
            get { return _selectReferencePlanesModel; }
            set { _selectReferencePlanesModel = value; }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private string _inputDistanceStepString;
        public string DistanceStepString
        {
            get { return _inputDistanceStepString; }
            set
            {
                _inputDistanceStepString = value;
                OnPropertyChanged(nameof(DistanceStepString));
            }
        }
        private double _distanceStep;
        public double DistanceStep
        {
            get { return _distanceStep; }
            set { _distanceStep = value; }
        }

        private string _distanceToleranceString;
        public string DistanceToleranceString
        {
            get { return _distanceToleranceString; }
            set
            {
                _distanceToleranceString = value;
                OnPropertyChanged(nameof(DistanceToleranceString));
            }
        }
        private double _distanceTolerance;
        public double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set { _distanceTolerance = value; }
        }

        private Color _filterColorAngle;
        public Color FilterColorAngle
        {
            get { return _filterColorAngle; }
            set
            {
                _filterColorAngle = value;
                OnPropertyChanged(nameof(FilterColorAngle));
            }
        }

        private Color _filterColorDistance;
        public Color FilterColorDistance
        {
            get { return _filterColorDistance; }
            set
            {
                _filterColorDistance = value;
                OnPropertyChanged(nameof(FilterColorDistance));
            }
        }

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

        private string _selectedElementsString;
        public string SelectedElementsString
        {
            get { return _selectedElementsString; }
            set
            {
                _selectedElementsString = value;
                OnPropertyChanged(nameof(SelectedElementsString));
            }
        }

        private string _selectedElementsError;
        public string SelectedElementsError
        {
            get { return _selectedElementsError; }
            set
            {
                _selectedElementsError = value;
                OnPropertyChanged(nameof(SelectedElementsError));
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsArrangedVM"/> class.
        /// </summary>
        public WallsArrangedVM(WallsArrangedM model, SelectReferencePlanesM selectModel)
        {
            Model = model;
            SelectReferencePlanesModel = selectModel;

            IsVisible = true;

            DistanceStep = 1;
            DistanceStepString = DistanceStep.ToString();
            DistanceTolerance = 0.000000001; // cm (enough is 0.000000030 - 0.000000035 - closer walls after join will be without dividing line)
            DistanceToleranceString = DistanceTolerance.ToString("0.000000000", NumberFormatInfo.CurrentInfo);
            FilterColorAngle = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };
            FilterColorDistance = new Color
            {
                R = 255,
                G = 64,
                B = 64
            };

            SelectedElements = new List<int> { 0, 0 };
            SelectedElementsString = "No selection";

            RunCommand = new RunCommand(RunAction);
            SelectReferencePlanesCommand = new RunCommand(SelectReferencePlanesAction);
        }

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region VALIDATION

        public string Error { get { return null; } }

        public string this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

        string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "DistanceStepString":
                    error = ValidateInputIsPointNumber(out double distanceStep, DistanceStepString);
                    if (string.IsNullOrEmpty(error))
                    {
                        DistanceStep = distanceStep;
                        error = ValidateResultDistanceStep();
                    }
                    break;
                case "DistanceToleranceString":
                    error = ValidateInputIsPointNumber(out double distanceTolerance, DistanceToleranceString);
                    if (string.IsNullOrEmpty(error))
                    {
                        DistanceTolerance = distanceTolerance;
                        error = ValidateResultDistanceTolerance();
                    }
                    break;
                case "SelectedElementsString":
                    if (SelectReferencePlanesModel.Error?.Length > 0)
                        error = SelectReferencePlanesModel.Error;
                    if (SelectedElements[0] == 0)
                        error = "No selection";
                    break;
            }
            return error;
        }

        private string ValidateInputIsPointNumber(out double numberParsed, string number)
        {
            numberParsed = 0;

            if (string.IsNullOrEmpty(number))
                return "Input is empty";
            if (!double.TryParse(number, out numberParsed))
                return "Invalid input";

            return null;
        }

        private string ValidateResultDistanceStep()
        {
            if (DistanceStep == 0)
                return "Cannot be 0";
            return null;
        }

        private string ValidateResultDistanceTolerance()
        {
            if (DistanceTolerance < _distanceToleranceMinValue)
            {
                string value = _distanceToleranceMinValue.ToString(_distanceToleranceMinValueFormat, NumberFormatInfo.CurrentInfo);
                return $"Cannot be lower than {value} cm";
            }
            else if (DistanceTolerance > _distanceToleranceMaxValue)
            {
                string value = _distanceToleranceMaxValue.ToString(_distanceToleranceMaxValueFormat, NumberFormatInfo.CurrentInfo);
                return $"Cannot be bigger than {value} cm";
            }
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.DistanceStepCm = DistanceStep;
            Model.DistanceToleranceCm = DistanceTolerance;
            Model.FilterColorAngleSystem = FilterColorAngle;
            Model.FilterColorDistanceSystem = FilterColorDistance;
            Model.SelectedElements = SelectedElements;

            Model.Run();
        }

        public ICommand SelectReferencePlanesCommand { get; set; }

        private void SelectReferencePlanesAction()
        {
            IsVisible = false;

            SelectReferencePlanesModel.Run();

            SelectedElements = SelectReferencePlanesModel.SelectedElements;

            SelectedElementsString = (SelectedElements[0] == 0)
                ? "No selection"
                : $"{SelectedElements[0]}; {SelectedElements[1]}";
            SelectedElementsError = SelectReferencePlanesModel.Error;

            IsVisible = true;
        }

        #endregion
    }
}