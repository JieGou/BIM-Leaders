using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "NamesChange"
    /// </summary>
    public class NamesChangeData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="NamesChangeData"/> class.
        /// </summary>
        public NamesChangeData()
        {
            ResultSubstringOld = "OLD";
            ResultSubstringNew = "NEW";
            ResultPartPrefix = true;
            ResultCategories = Enumerable.Repeat(false, 24).ToList();
            ResultCategories[6] = true;
        }

        private string _resultSubstringOld;
        public string ResultSubstringOld
        {
            get { return _resultSubstringOld; }
            set
            {
                _resultSubstringOld = value;
                OnPropertyChanged(nameof(ResultSubstringOld));
            }
        }

        private string _resultSubstringNew;
        public string ResultSubstringNew
        {
            get { return _resultSubstringNew; }
            set
            {
                _resultSubstringNew = value;
                OnPropertyChanged(nameof(ResultSubstringNew));
            }
        }

        private bool _resultPartPrefix { get; set; }
        public bool ResultPartPrefix
        {
            get { return _resultPartPrefix; }
            set
            {
                _resultPartPrefix = value;
                OnPropertyChanged(nameof(ResultPartPrefix));
            }
        }
        private bool _resultPartCenter { get; set; }
        public bool ResultPartCenter
        {
            get { return _resultPartCenter; }
            set
            {
                _resultPartCenter = value;
                OnPropertyChanged(nameof(ResultPartCenter));
            }
        }
        private bool _resultPartSuffix { get; set; }
        public bool ResultPartSuffix
        {
            get { return _resultPartSuffix; }
            set
            {
                _resultPartSuffix = value;
                OnPropertyChanged(nameof(ResultPartSuffix));
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
                case "ResultSubstringOld":
                    error = ValidateResultSubstringOld();
                    break;
                case "ResultSubstringNew":
                    error = ValidateResultSubstringNew();
                    break;
                case "ResultCategories":
                    error = ValidateResultCategories();
                    break;
            }
            return error;
        }

        private string ValidateResultSubstringOld()
        {
            if (string.IsNullOrEmpty(ResultSubstringOld))
                return "Input is empty";
            else
            {
                if (ResultSubstringOld.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateResultSubstringNew()
        {
            if (string.IsNullOrEmpty(ResultSubstringNew))
                return "Input is empty";
            else
            {
                if (ResultSubstringNew.Length < 2)
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
