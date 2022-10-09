using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Linetypes_IMPORT_Delete"
    /// </summary>
    public class PurgeVM : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="PurgeVM"/> class.
        /// </summary>
        public PurgeVM()
        {
            ResultRooms = true;
            ResultTags = true;
            ResultFilters = true;
            ResultViewTemplates = true;
            ResultSheets = true;
            ResultLineStyles = true;
            ResultLinePatterns = true;
            ResultLinePatternsName = "IMPORT";
        }

        private bool _resultRooms;
        public bool ResultRooms
        {
            get { return _resultRooms; }
            set
            {
                _resultRooms = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultTags;
        public bool ResultTags
        {
            get { return _resultTags; }
            set
            {
                _resultTags = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultFilters;
        public bool ResultFilters
        {
            get { return _resultFilters; }
            set
            {
                _resultFilters = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultViewTemplates;
        public bool ResultViewTemplates
        {
            get { return _resultViewTemplates; }
            set
            {
                _resultViewTemplates = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultSheets;
        public bool ResultSheets
        {
            get { return _resultSheets; }
            set
            {
                _resultSheets = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultLineStyles;
        public bool ResultLineStyles
        {
            get { return _resultLineStyles; }
            set
            {
                _resultLineStyles = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private bool _resultLinePatterns;
        public bool ResultLinePatterns
        {
            get { return _resultLinePatterns; }
            set
            {
                _resultLinePatterns = value;
                OnPropertyChanged(nameof(ResultRooms));
                OnPropertyChanged(nameof(ResultTags));
                OnPropertyChanged(nameof(ResultFilters));
                OnPropertyChanged(nameof(ResultViewTemplates));
                OnPropertyChanged(nameof(ResultSheets));
                OnPropertyChanged(nameof(ResultLineStyles));
                OnPropertyChanged(nameof(ResultLinePatterns));
            }
        }

        private string _resultLinePatternsName;
        public string ResultLinePatternsName
        {
            get { return _resultLinePatternsName; }
            set
            {
                _resultLinePatternsName = value;
                OnPropertyChanged(nameof(ResultLinePatternsName));
            }
        }

        // Dictionary for property (key) and error
        //private Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();
        public string this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

        #region Validation

        public string Error { get { return null; } }

        string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "ResultRooms":
                    error = ValidateResult(); break;
                case "ResultTags":
                    error = ValidateResult(); break;
                case "ResultFilters":
                    error = ValidateResult(); break;
                case "ResultViewTemplates":
                    error = ValidateResult(); break;
                case "ResultSheets":
                    error = ValidateResult(); break;
                case "ResultLineStyles":
                    error = ValidateResult(); break;
                case "ResultLinePatterns":
                    error = ValidateResult(); break;
                case "ResultLinePatternsName":
                    error = ValidateResultName(); break;
            }
            return error;
        }

        private string ValidateResult()
        {
            if (ResultRooms == false
                && ResultTags == false
                && ResultFilters == false
                && ResultViewTemplates == false
                && ResultSheets == false
                && ResultLineStyles == false
                && ResultLinePatterns == false)
                return "Check at least one purge item";
            return null;
        }

        private string ValidateResultName()
        {
            if (string.IsNullOrEmpty(ResultLinePatternsName))
                return "Input is empty";
            else
            {
                if (ResultLinePatternsName.Length < 3)
                    return "From 3 symbols";
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
