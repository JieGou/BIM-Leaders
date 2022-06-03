using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class DimensionStairsLandingsData : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _resultDistanceMinValue = 100;
        private const int _resultDistanceMaxValue = 200;

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionStairsLandingsData"/> class.
        /// </summary>
        public DimensionStairsLandingsData()
        {
            _resultDistance = 150;
            _inputDistance = _resultDistance.ToString();
        }


        private string _inputDistance;
        public string InputDistance
        {
            get { return _inputDistance; }
            set
            {
                _inputDistance = value;
                OnPropertyChanged(nameof(InputDistance));
            }
        }
        private int _resultDistance;
        public int ResultDistance
        {
            get { return _resultDistance; }
            set { _resultDistance = value; }
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
                case "InputDistance":
                    error = ValidateInputIsWholeNumber(out int distance, _inputDistance);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultDistance = distance;
                        error = ValidateResultDistance();
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

        private string ValidateResultDistance()
        {
            if (ResultDistance < _resultDistanceMinValue || ResultDistance > _resultDistanceMaxValue)
                return $"From {_resultDistanceMinValue} to {_resultDistanceMaxValue} cm";
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
