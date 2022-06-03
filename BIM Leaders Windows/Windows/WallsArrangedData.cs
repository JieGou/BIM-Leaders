using System.ComponentModel;
using System.Windows.Media;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class WallsArrangedData : INotifyPropertyChanged, IDataErrorInfo
    {
        private const double _resultDistanceToleranceMinValue = 0.000000000001;
        private const double _resultDistanceToleranceMaxValue = 0.1;

        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsArrangedData"/> class.
        /// </summary>
        public WallsArrangedData()
        {
            _resultDistanceStep = 1;
            _inputDistanceStep = _resultDistanceStep.ToString();
            _resultDistanceTolerance = 0.000000001; // cm (enough is 0.000000030 - 0.000000035 - closer walls after join will be without dividing line)
            _inputDistanceTolerance = _resultDistanceTolerance.ToString();
            _resultColor0 = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };
            _resultColor1 = new Color
            {
                R = 255,
                G = 64,
                B = 64
            };
        }

        private string _inputDistanceStep;
        public string InputDistanceStep
        {
            get { return _inputDistanceStep; }
            set
            {
                _inputDistanceStep = value;
                OnPropertyChanged(nameof(InputDistanceStep));
            }
        }
        private double _resultDistanceStep;
        public double ResultDistanceStep
        {
            get { return _resultDistanceStep; }
            set { _resultDistanceStep = value; }
        }

        private string _inputDistanceTolerance;
        public string InputDistanceTolerance
        {
            get { return _inputDistanceTolerance; }
            set
            {
                _inputDistanceTolerance = value;
                OnPropertyChanged(nameof(InputDistanceTolerance));
            }
        }
        private double _resultDistanceTolerance;
        public double ResultDistanceTolerance
        {
            get { return _resultDistanceTolerance; }
            set { _resultDistanceTolerance = value; }
        }

        private Color _resultColor0;
        public Color ResultColor0
        {
            get { return _resultColor0; }
            set
            {
                _resultColor0 = value;
                OnPropertyChanged(nameof(ResultColor0));
            }
        }

        private Color _resultColor1;
        public Color ResultColor1
        {
            get { return _resultColor1; }
            set
            {
                _resultColor1 = value;
                OnPropertyChanged(nameof(ResultColor1));
            }
        }

        public string this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }


        #region Validation

        string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "InputDistanceStep":
                    error = ValidateInputIsPointNumber(out double distanceStep, _inputDistanceStep);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultDistanceStep = distanceStep;
                        error = ValidateResultDistanceStep();
                    }
                    break;
                case "InputDistanceTolerance":
                    error = ValidateInputIsPointNumber(out double distanceTolerance, _inputDistanceTolerance);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultDistanceTolerance = distanceTolerance;
                        error = ValidateResultDistanceTolerance();
                    }
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
            if (ResultDistanceStep == 0)
                return "Cannot be 0";
            return null;
        }

        private string ValidateResultDistanceTolerance()
        {
            if (ResultDistanceTolerance < _resultDistanceToleranceMinValue)
                return $"Cannot be lower than {_resultDistanceToleranceMinValue} cm";
            else if (ResultDistanceTolerance > _resultDistanceToleranceMaxValue)
                return $"Cannot be bigger than {_resultDistanceToleranceMaxValue} cm";
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
