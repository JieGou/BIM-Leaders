using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Linetypes_IMPORT_Delete"
    /// </summary>
    public class Linetypes_Delete_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Linetypes_Delete_Data"/> class.
        /// </summary>
        public Linetypes_Delete_Data()
        {
            result_name = "IMPORT";
        }

        private string _result_name;
        public string result_name
        {
            get { return _result_name; }
            set
            {
                _result_name = value;
                OnPropertyChanged(nameof(result_name));
            }
        }

        // Dictionary for property (key) and error
        //private Dictionary<string, string> Errors { get; } = new Dictionary<string, string>();
        public string this[string property_name]
        {
            get
            {
                return GetValidationError(property_name);
            }
        }


        #region Validation

        string GetValidationError(string property_name)
        {
            string error = null;
            
            switch (property_name)
            {
                case "result_name":
                    error = Validate_result_name();
                    break;
            }
            return error;
        }

        private string Validate_result_name()
        {
            if (string.IsNullOrEmpty(result_name))
                return "Input is empty";
            else
            {
                if (result_name.Length < 3)
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
