﻿using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DimensionsPlan"
    /// </summary>
    public class DimensionsPlanData : INotifyPropertyChanged, IDataErrorInfo
    {
        int _resultSearchStepMinValue = 1;
        int _resultSearchStepMaxValue = 100;
        int _resultSearchDistanceMinValue = 100;
        int _resultSearchDistanceMaxValue = 10000;

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionsPlanData"/> class.
        /// </summary>
        public DimensionsPlanData()
        {
            _resultSearchStep = 15;
            _inputSearchStep = _resultSearchStep.ToString();
            _resultSearchDistance = 1500;
            _inputSearchDistance = _resultSearchDistance.ToString();
        }

        private string _inputSearchStep;
        public string InputSearchStep
        {
            get { return _inputSearchStep; }
            set
            {
                _inputSearchStep = value;
                OnPropertyChanged(nameof(InputSearchStep));
            }
        }
        private int _resultSearchStep;
        public int ResultSearchStep
        {
            get { return _resultSearchStep; }
            set { _resultSearchStep = value; }
        }

        private string _inputSearchDistance;
        public string InputSearchDistance
        {
            get { return _inputSearchDistance; }
            set
            {
                _inputSearchDistance = value;
                OnPropertyChanged(nameof(InputSearchDistance));
            }
        }
        private int _resultSearchDistance;
        public int ResultSearchDistance
        {
            get { return _resultSearchDistance; }
            set { _resultSearchDistance = value; }
        }

        #region Validation

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
                case "InputSearchStep":
                    error = ValidateInputIsWholeNumber(out int searchStep, _inputSearchStep);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultSearchStep = searchStep;
                        error = ValidateResultSearchStep();
                    }
                    break;
                case "InputSearchDistance":
                    error = ValidateInputIsWholeNumber(out int searchDistance, _inputSearchDistance);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultSearchDistance = searchDistance;
                        error = ValidateResultSearchDistance();
                    }
                    break;
            }
            return error;
        }

        private string ValidateInputIsWholeNumber(out int numberParsed, string number)
        {
            numberParsed = 0;

            if (string.IsNullOrEmpty(number))
                return "Input is empty";
            if (!int.TryParse(number, out numberParsed))
                return "Not a whole number";

            return null;
        }

        private string ValidateResultSearchStep()
        {
            if (ResultSearchStep < _resultSearchStepMinValue || ResultSearchStep > _resultSearchStepMaxValue)
                return $"From {_resultSearchStepMinValue} to {_resultSearchStepMaxValue} cm";
            return null;
        }

        private string ValidateResultSearchDistance()
        {
            if (ResultSearchDistance < _resultSearchDistanceMinValue || ResultSearchDistance > _resultSearchDistanceMaxValue)
                return $"From {_resultSearchDistanceMinValue} to {_resultSearchDistanceMaxValue} cm";
            return null;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
