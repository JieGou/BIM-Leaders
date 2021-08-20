using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Names_Prefix_Change"
    /// </summary>
    public class Checker_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Checker_Data"/> class.
        /// </summary>
        public Checker_Data()
        {
            result_prefix = "PRE_";
            result_categories = Enumerable.Repeat(true, 24).ToList();
            result_model = Enumerable.Repeat(true, 5).ToList();
            result_codes = Enumerable.Repeat(true, 2).ToList();
            result_height = 210;
        }

        private string _result_prefix;
        public string result_prefix
        {
            get { return _result_prefix; }
            set
            {
                _result_prefix = value;
                OnPropertyChanged(nameof(result_prefix));
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

        private List<bool> _result_model { get; set; }
        public List<bool> result_model
        {
            get { return _result_model; }
            set
            {
                _result_model = value;
                OnPropertyChanged(nameof(result_model));
            }
        }

        private List<bool> _result_codes { get; set; }
        public List<bool> result_codes
        {
            get { return _result_codes; }
            set
            {
                _result_codes = value;
                OnPropertyChanged(nameof(result_codes));
            }
        }

        private int _result_height { get; set; }
        public int result_height
        {
            get { return _result_height; }
            set
            {
                _result_height = value;
                OnPropertyChanged(nameof(result_height));
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
                case "result_prefix":
                    error = Validate_result_prefix();
                    break;
                case "checkboxes":
                    error = Validate_result_checkboxes();
                    break;
                case "codes":
                    error = Validate_result_codes();
                    break;
            }
            return error;
        }

        private string Validate_result_prefix()
        {
            if (string.IsNullOrEmpty(result_prefix))
                return "Naming tab - Input is empty";
            else
            {
                if (result_prefix.Length < 2)
                    return "Naming tab - From 2 symbols";
            }
            return null;
        }

        private string Validate_result_checkboxes()
        {
            if (!result_categories.Contains(true) && !result_model.Contains(true) && !result_codes.Contains(true))
                return "Check at least one item";
            return null;
        }

        private string Validate_result_codes()
        {
            if (result_height < 200)
                return "Codes tab - Height check must be over 200 cm";
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
