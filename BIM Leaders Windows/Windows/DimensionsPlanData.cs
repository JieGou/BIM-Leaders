using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DimensionsPlan"
    /// </summary>
    public class DimensionsPlanData : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionsPlanData"/> class.
        /// </summary>
        public DimensionsPlanData()
        {
            ResultSearchStep = "15";
            ResultSearchDistance = "1500";
        }

        private string _resultSearchStep;
        public string ResultSearchStep
        {
            get { return _resultSearchStep; }
            set
            {
                _resultSearchStep = value;
                OnPropertyChanged(nameof(ResultSearchStep));
            }
        }

        private string _resultSearchDistance;
        public string ResultSearchDistance
        {
            get { return _resultSearchDistance; }
            set
            {
                _resultSearchDistance = value;
                OnPropertyChanged(nameof(ResultSearchDistance));
            }
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
                case "ResultSearchStep":
                    error = ValidateResultSearchStep();
                    break;
                case "ResultSearchDistance":
                    error = ValidateResultSearchDistance();
                    break;
            }
            return error;
        }

        private string ValidateResultSearchStep()
        {
            if (string.IsNullOrEmpty(ResultSearchStep))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultSearchStep, out int y))
                {
                    if (y < 1 || y > 100)
                        return "From 1 to 100 cm";
                }
                else
                    return "Invalid input";
            }
            return null;
        }

        private string ValidateResultSearchDistance()
        {
            if (string.IsNullOrEmpty(ResultSearchStep))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultSearchStep, out int y))
                {
                    if (y < 100 || y > 10000)
                        return "From 100 to 10000 cm";
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
