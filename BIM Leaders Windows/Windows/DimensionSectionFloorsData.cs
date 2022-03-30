using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class DimensionSectionFloorsData : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionSectionFloorsData"/> class.
        /// </summary>
        public DimensionSectionFloorsData()
        {
            ResultSpots = true;
            ResultPlacement = Enumerable.Repeat(true, 3).ToList();
            ResultThickness = "10";
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

        private string _resultThickness;
        public string ResultThickness
        {
            get { return _resultThickness; }
            set
            {
                _resultThickness = value;
                OnPropertyChanged(nameof(ResultThickness));
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
                case "ResultPlacement":
                    error = ValidateResultPlacement();
                    break;
                case "ResultThickness":
                    error = ValidateResultThickness();
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

        private string ValidateResultThickness()
        {
            if (string.IsNullOrEmpty(ResultThickness))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultThickness, out int y))
                {
                    if (y < 1 || y > 100)
                        return "From 1 to 100 cm";
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
