using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command DimensionStairsLandings
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
            _resultPlacementDimensionTop = true;
            _resultPlacementDimensionMid = true;
            _resultPlacementDimensionBot = true;
            _resultPlacementElevationTop = true;
            _resultPlacementElevationMid = true;
            _resultPlacementElevationBot = true;
            _resultDistance = 150;
            _inputDistance = _resultDistance.ToString();
        }

        private bool _resultPlacementDimensionTop;
        public bool ResultPlacementDimensionTop
        {
            get { return _resultPlacementDimensionTop; }
            set
            {
                _resultPlacementDimensionTop = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
        }

        private bool _resultPlacementDimensionMid;
        public bool ResultPlacementDimensionMid
        {
            get { return _resultPlacementDimensionMid; }
            set
            {
                _resultPlacementDimensionMid = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
        }

        private bool _resultPlacementDimensionBot;
        public bool ResultPlacementDimensionBot
        {
            get { return _resultPlacementDimensionBot; }
            set
            {
                _resultPlacementDimensionBot = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
        }

        private bool _resultPlacementElevationTop;
        public bool ResultPlacementElevationTop
        {
            get { return _resultPlacementElevationTop; }
            set
            {
                _resultPlacementElevationTop = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
        }

        private bool _resultPlacementElevationMid;
        public bool ResultPlacementElevationMid
        {
            get { return _resultPlacementElevationMid; }
            set
            {
                _resultPlacementElevationMid = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
        }

        private bool _resultPlacementElevationBot;
        public bool ResultPlacementElevationBot
        {
            get { return _resultPlacementElevationBot; }
            set
            {
                _resultPlacementElevationBot = value;
                OnPropertyChanged(nameof(ResultPlacementDimensionTop));
                OnPropertyChanged(nameof(ResultPlacementDimensionMid));
                OnPropertyChanged(nameof(ResultPlacementDimensionBot));
                OnPropertyChanged(nameof(ResultPlacementElevationTop));
                OnPropertyChanged(nameof(ResultPlacementElevationMid));
                OnPropertyChanged(nameof(ResultPlacementElevationBot));
            }
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
                case "ResultPlacementDimensionTop":
                    error = ValidateResultPlacement();
                    break;
                case "ResultPlacementDimensionMid":
                    error = ValidateResultPlacement();
                    break;
                case "ResultPlacementDimensionBot":
                    error = ValidateResultPlacement();
                    break;
                case "ResultPlacementElevationTop":
                    error = ValidateResultPlacement();
                    break;
                case "ResultPlacementElevationMid":
                    error = ValidateResultPlacement();
                    break;
                case "ResultPlacementElevationBot":
                    error = ValidateResultPlacement();
                    break;
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

        private string ValidateResultPlacement()
        {
            if (ResultPlacementDimensionTop == false && ResultPlacementDimensionMid == false
                && ResultPlacementDimensionBot == false && ResultPlacementElevationTop == false
                && ResultPlacementElevationMid == false && ResultPlacementElevationBot == false)
                return "Check at least one placement";
            return null;
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
