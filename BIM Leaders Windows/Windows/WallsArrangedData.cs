using System.ComponentModel;
using System.Windows.Media;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class WallsArrangedData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsArrangedData"/> class.
        /// </summary>
        public WallsArrangedData()
        {
            ResultDistanceStep = "1"; // 1 cm
            ResultDistanceTolerance = "0.0000001"; // 0.00001 cm
            ResultColor0 = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };
            ResultColor1 = new Color
            {
                R = 255,
                G = 64,
                B = 64
            };
        }

        private string _resultDistanceStep;
        public string ResultDistanceStep
        {
            get { return _resultDistanceStep; }
            set
            {
                _resultDistanceStep = value;
                OnPropertyChanged(nameof(ResultDistanceStep));
            }
        }

        private string _resultDistanceTolerance;
        public string ResultDistanceTolerance
        {
            get { return _resultDistanceTolerance; }
            set
            {
                _resultDistanceTolerance = value;
                OnPropertyChanged(nameof(ResultDistanceTolerance));
            }
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
                case "ResultDistanceStep":
                    error = ValidateResultDistanceStep();
                    break;
                case "ResultDistanceTolerance":
                    error = ValidateResultDistanceTolerance();
                    break;
            }
            return error;
        }

        private string ValidateResultDistanceStep()
        {
            if (string.IsNullOrEmpty(ResultDistanceStep))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultDistanceStep, out int x))
                {
                    if (x == 0)
                        return "Cannot be 0";
                }
                else
                    return "Invalid input";
            }
            return null;
        }

        private string ValidateResultDistanceTolerance()
        {
            if (string.IsNullOrEmpty(ResultDistanceTolerance))
                return "Input is empty";
            else
            {
                if (double.TryParse(ResultDistanceTolerance, out double x))
                {
                    if (x < 0.000000000001)
                        return "Cannot be lower than 0.000000000001 cm";
                    else if (x > 0.1)
                        return "Cannot be bigger than 0.1 cm";
                }
                else
                    return "Invalid input";
            }
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
