using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Linetypes_IMPORT_Delete"
    /// </summary>
    public class LinetypesDeleteData : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="LinetypesDeleteData"/> class.
        /// </summary>
        public LinetypesDeleteData()
        {
            ResultName = "IMPORT";
        }

        private string _resultName;
        public string ResultName
        {
            get { return _resultName; }
            set
            {
                _resultName = value;
                OnPropertyChanged(nameof(ResultName));
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

        string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "ResultName":
                    error = ValidateResultName();
                    break;
            }
            return error;
        }

        private string ValidateResultName()
        {
            if (string.IsNullOrEmpty(ResultName))
                return "Input is empty";
            else
            {
                if (ResultName.Length < 3)
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
