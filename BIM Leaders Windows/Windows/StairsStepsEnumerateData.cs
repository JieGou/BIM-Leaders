using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Stairs_Steps_Enumerate"
    /// </summary>
    public class StairsStepsEnumerateData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="StairsStepsEnumerateData"/> class.
        /// </summary>
        public StairsStepsEnumerateData()
        {
            ResultSideRight = true;
            ResultNumber = "1";
        }

        private bool _resultSideRight;
        public bool ResultSideRight
        {
            get { return _resultSideRight; }
            set
            {
                _resultSideRight = value;
                OnPropertyChanged(nameof(ResultSideRight));
            }
        }

        private string _resultNumber;
        public string ResultNumber
        {
            get { return _resultNumber; }
            set
            {
                _resultNumber = value;
                OnPropertyChanged(nameof(ResultNumber));
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
                case "ResultNumber":
                    error = ValidateResultNumber();
                    break;
            }
            return error;
        }

        private string ValidateResultNumber()
        {
            if (string.IsNullOrEmpty(ResultNumber))
                return "Input is empty";
            else
            {
                if (int.TryParse(ResultNumber, out int y))
                {
                    if (y < 0)
                        return "From 0";
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
