using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class DimensionSectionFloorsData : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _resultThicknessMinValue = 1;
        private const int _resultThicknessMaxValue = 100;
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionSectionFloorsData"/> class.
        /// </summary>
        public DimensionSectionFloorsData()
        {
            _resultSpots = true;
            _resultPlacement = Enumerable.Repeat(true, 3).ToList();
            _resultThickness = 10;
            _inputThickness = _resultThickness.ToString();
        }

        private bool _resultSpots;
        public bool ResultSpots
        {
            get { return _resultSpots; }
            set
            {
                _resultSpots = value;
                OnPropertyChanged(nameof(ResultSpots));
            }
        }

        private List<bool> _resultPlacement;
        public List<bool> ResultPlacement
        {
            get { return _resultPlacement; }
            set
            {
                _resultPlacement = value;
                OnPropertyChanged(nameof(ResultPlacement));
            }
        }

        private string _inputThickness;
        public string InputThickness
        {
            get { return _inputThickness; }
            set
            {
                _inputThickness = value;
                OnPropertyChanged(nameof(InputThickness));
            }
        }
        private int _resultThickness;
        public int ResultThickness
        {
            get { return _resultThickness; }
            set { _resultThickness = value; }
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
                case "ResultPlacement":
                    error = ValidateResultPlacement();
                    break;
                case "InputThickness":
                    error = ValidateInputIsWholeNumber(out int thickness, _inputThickness);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultThickness = thickness;
                        error = ValidateResultThickness();
                    }   
                    break;
            }
            return error;
        }

        private string ValidateResultPlacement()
        {
            if (!ResultPlacement.Contains(true))
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

        private string ValidateResultThickness()
        {
            if (ResultThickness < _resultThicknessMinValue || ResultThickness > _resultThicknessMaxValue)
                return $"From {_resultThicknessMinValue} to {_resultThicknessMaxValue} cm";
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
