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
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionStairsLandingsData"/> class.
        /// </summary>
        public DimensionStairsLandingsData()
        {
            ResultDistance = "150";
        }

        private string _resultDistance;
        public string ResultDistance
        {
            get { return _resultDistance; }
            set
            {
                _resultDistance = value;
                OnPropertyChanged(nameof(ResultDistance));
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
                case "ResultDistance":
                    error = ValidateResultDistance();
                    break;
            }
            return error;
        }

        private string ValidateResultDistance()
        {
            if (string.IsNullOrEmpty(ResultDistance))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultDistance, out int y))
                {
                    if (y < 100 || y > 200)
                        return "From 100 to 200 cm";
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
