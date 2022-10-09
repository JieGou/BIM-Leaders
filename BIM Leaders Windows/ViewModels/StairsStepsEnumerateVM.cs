using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Stairs_Steps_Enumerate"
    /// </summary>
    public class StairsStepsEnumerateVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _resultNumberMinValue = 0;

        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="StairsStepsEnumerateVM"/> class.
        /// </summary>
        public StairsStepsEnumerateVM()
        {
            _resultSideRight = true;
            _resultNumber = 1;
            _inputNumber = _resultNumber.ToString();
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

        private string _inputNumber;
        public string InputNumber
        {
            get { return _inputNumber; }
            set
            {
                _inputNumber = value;
                OnPropertyChanged(nameof(InputNumber));
            }
        }
        private int _resultNumber;
        public int ResultNumber
        {
            get { return _resultNumber; }
            set { _resultNumber = value; }
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
                case "InputNumber":
                    error = ValidateInputIsWholeNumber(out int number, _inputNumber);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultNumber = number;
                        error = ValidateResultNumber();
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

        private string ValidateResultNumber()
        {
            if (ResultNumber < _resultNumberMinValue)
                return $"From {_resultNumberMinValue}";
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
