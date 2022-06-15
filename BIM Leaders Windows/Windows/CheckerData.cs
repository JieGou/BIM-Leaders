using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Names_Prefix_Change"
    /// </summary>
    public class CheckerData : INotifyPropertyChanged, IDataErrorInfo
    {
        // Minimal height that can be accepted.
        private const int _resultHeightMinValue = 200;

        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="CheckerData"/> class.
        /// </summary>
        public CheckerData()
        {
            _resultPrefix = "PRE_";
            _resultCategories = Enumerable.Repeat(false, 24).ToList();
            _resultCategories[6] = true;
            _resultModel = Enumerable.Repeat(false, 14).ToList();
            _resultCodes = Enumerable.Repeat(false, 2).ToList();
            _resultHeight = 210;
            _inputHeight = _resultHeight.ToString();
        }

        private string _resultPrefix;
        public string ResultPrefix
        {
            get { return _resultPrefix; }
            set
            {
                _resultPrefix = value;
                OnPropertyChanged(nameof(ResultPrefix));
            }
        }

        private List<bool> _resultCategories { get; set; }
        public List<bool> ResultCategories
        {
            get { return _resultCategories; }
            set
            {
                _resultCategories = value;
                OnPropertyChanged(nameof(ResultCategories));
            }
        }

        private List<bool> _resultModel { get; set; }
        public List<bool> ResultModel
        {
            get { return _resultModel; }
            set
            {
                _resultModel = value;
                OnPropertyChanged(nameof(ResultModel));
            }
        }

        private List<bool> _resultCodes { get; set; }
        public List<bool> ResultCodes
        {
            get { return _resultCodes; }
            set
            {
                _resultCodes = value;
                OnPropertyChanged(nameof(ResultCodes));
            }
        }

        private string _inputHeight { get; set; }
        public string InputHeight
        {
            get { return _inputHeight; }
            set
            {
                _inputHeight = value;
                OnPropertyChanged(nameof(InputHeight));
            }
        }
        private int _resultHeight { get; set; }
        public int ResultHeight
        {
            get { return _resultHeight; }
            set { _resultHeight = value; }
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
                case "ResultPrefix":
                    error = ValidateResultPrefix();
                    break;
                case "InputHeight":
                    error = ValidateInputIsWholeNumber(out int height, _inputHeight);
                    if (string.IsNullOrEmpty(error))
                    {
                        ResultHeight = height;
                        error = ValidateResultHeight();
                    }
                    break;
                default:
                    break;
            }
            return error;
        }

        private string ValidateResultPrefix()
        {
            if (string.IsNullOrEmpty(ResultPrefix))
                return "Input is empty";
            else
            {
                if (ResultPrefix.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateResultCheckboxes()
        {
            if (!ResultCategories.Contains(true) && !ResultModel.Contains(true) && !ResultCodes.Contains(true))
                return "Check at least one item";
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

        private string ValidateResultHeight()
        {
            if (_resultHeight < _resultHeightMinValue)
                return $"Must be over {_resultHeightMinValue} cm";
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
