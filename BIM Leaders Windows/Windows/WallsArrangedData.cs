using System.ComponentModel;

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
            ResultDistanceTolerance = "0.00001"; // 0.00001 cm
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
