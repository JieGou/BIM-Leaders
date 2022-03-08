using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Names_Prefix_Change"
    /// </summary>
    public class NamesPrefixChangeData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="NamesPrefixChangeData"/> class.
        /// </summary>
        public NamesPrefixChangeData()
        {
            ResultPrefixOld = "OLD";
            ResultPrefixNew = "NEW";
            ResultCategories = Enumerable.Repeat(true, 24).ToList();
        }

        private string _resultPrefixOld;
        public string ResultPrefixOld
        {
            get { return _resultPrefixOld; }
            set
            {
                _resultPrefixOld = value;
                OnPropertyChanged(nameof(ResultPrefixOld));
            }
        }

        private string _resultPrefixNew;
        public string ResultPrefixNew
        {
            get { return _resultPrefixNew; }
            set
            {
                _resultPrefixNew = value;
                OnPropertyChanged(nameof(ResultPrefixNew));
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
                case "ResultPrefixOld":
                    error = ValidateResultPrefixOld();
                    break;
                case "ResultPrefixNew":
                    error = ValidateResultPrefixNew();
                    break;
                case "ResultCategories":
                    error = ValidateResultCategories();
                    break;
            }
            return error;
        }

        private string ValidateResultPrefixOld()
        {
            if (string.IsNullOrEmpty(ResultPrefixOld))
                return "Input is empty";
            else
            {
                if (ResultPrefixOld.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateResultPrefixNew()
        {
            if (string.IsNullOrEmpty(ResultPrefixNew))
                return "Input is empty";
            else
            {
                if (ResultPrefixNew.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateResultCategories()
        {
            if (!ResultCategories.Contains(true))
                return "Categories not checked";
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
