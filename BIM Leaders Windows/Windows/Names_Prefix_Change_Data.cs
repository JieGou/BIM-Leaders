using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Names_Prefix_Change"/>
    /// </summary>
    public class Names_Prefix_Change_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Names_Prefix_Change_Data"/> class.
        /// </summary>
        public Names_Prefix_Change_Data()
        {
            result_prefix_old = "OLD";
            result_prefix_new = "NEW";
            result_categories = Enumerable.Repeat(true, 24).ToList();
        }

        private string _result_prefix_old;
        public string result_prefix_old
        {
            get { return _result_prefix_old; }
            set
            {
                _result_prefix_old = value;
                OnPropertyChanged(nameof(result_prefix_old));
            }
        }

        private string _result_prefix_new;
        public string result_prefix_new
        {
            get { return _result_prefix_new; }
            set
            {
                _result_prefix_new = value;
                OnPropertyChanged(nameof(result_prefix_new));
            }
        }

        private List<bool> _result_categories { get; set; }
        public List<bool> result_categories
        {
            get { return _result_categories; }
            set
            {
                _result_categories = value;
                OnPropertyChanged(nameof(result_categories));
            }
        }

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
                case "result_prefix_old":
                    error = Validate_result_prefix_old();
                    break;
                case "result_prefix_new":
                    error = Validate_result_prefix_new();
                    break;
                case "categories":
                    error = Validate_result_categories();
                    break;
            }
            return error;
        }

        private string Validate_result_prefix_old()
        {
            if (string.IsNullOrEmpty(result_prefix_old))
                return "Input is empty";
            else
            {
                if (result_prefix_old.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string Validate_result_prefix_new()
        {
            if (string.IsNullOrEmpty(result_prefix_new))
                return "Input is empty";
            else
            {
                if (result_prefix_new.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string Validate_result_categories()
        {
            if (!result_categories.Contains(true))
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
