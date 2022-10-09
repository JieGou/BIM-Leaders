using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DimensionsPlan"
    /// </summary>
    public class DimensionsPlanVM : INotifyPropertyChanged, IDataErrorInfo
    {
        int _resultSearchStepMinValue = 1;
        int _resultSearchStepMaxValue = 100;
        int _resultSearchDistanceMinValue = 100;
        int _resultSearchDistanceMaxValue = 10000;
        int _resultMinReferencesMinValue = 0;
        int _resultMinReferencesMaxValue = 10;

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionsPlanVM"/> class.
        /// </summary>
        public DimensionsPlanVM()
        {
            _resultSearchStep = 15;
            _inputSearchStep = _resultSearchStep.ToString();
            _resultSearchDistance = 1500;
            _inputSearchDistance = _resultSearchDistance.ToString();
            _resultMinReferences = 5;
            _inputMinReferences = _resultMinReferences.ToString();
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

        private string _inputMinReferences;
        public string InputMinReferences
        {
            get { return _inputMinReferences; }
            set
            {
                _inputMinReferences = value;
                OnPropertyChanged(nameof(InputMinReferences));
            }
        }
        private int _resultMinReferences;
        public int ResultMinReferences
        {
            get { return _resultMinReferences; }
            set { _resultMinReferences = value; }
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
                case "InputMinReferences":
                    error = ValidateInputIsWholeNumber(out int minreferences, _inputMinReferences);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultMinReferences = minreferences;
                        error = ValidateResultMinReferences();
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

        private string ValidateResultMinReferences()
        {
            if (ResultMinReferences < _resultMinReferencesMinValue || ResultMinReferences > _resultMinReferencesMaxValue)
                return $"From {_resultMinReferencesMinValue} to {_resultMinReferencesMaxValue}";
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
