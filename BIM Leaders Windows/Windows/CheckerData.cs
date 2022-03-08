using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Names_Prefix_Change"
    /// </summary>
    public class CheckerData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="CheckerData"/> class.
        /// </summary>
        public CheckerData()
        {
            ResultPrefix = "PRE_";
            ResultCategories = Enumerable.Repeat(false, 24).ToList();
            ResultModel = Enumerable.Repeat(false, 5).ToList();
            ResultCodes = Enumerable.Repeat(false, 2).ToList();
            ResultHeight = 210;
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

        private int _resultHeight { get; set; }
        public int ResultHeight
        {
            get { return _resultHeight; }
            set
            {
                _resultHeight = value;
                OnPropertyChanged(nameof(ResultHeight));
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
                case "ResultPrefix":
                    error = ValidateResultPrefix();
                    break;
                case "ResultHeight":
                    error = ValidateResultCodes();
                    break;
                case "ResultCategories":
                    error = ValidateResultCheckboxes();
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

        private string ValidateResultCodes()
        {
            if (ResultHeight < 200)
                return "Must be over 200 cm";
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
