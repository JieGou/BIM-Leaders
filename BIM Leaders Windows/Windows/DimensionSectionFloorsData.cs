using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class DimensionSectionFloorsData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionSectionFloorsData"/> class.
        /// </summary>
        public DimensionSectionFloorsData()
        {
            ResultSpots = true;
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
                case "ResultThickness":
                    error = ValidateResultThickness();
                    break;
            }
            return error;
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
