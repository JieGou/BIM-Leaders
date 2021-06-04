using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Stairs_Steps_Enumerate"/>
    /// </summary>
    public class Stairs_Steps_Enumerate_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Stairs_Steps_Enumerate_Data"/> class.
        /// </summary>
        public Stairs_Steps_Enumerate_Data()
        {
            result_side_right = true;
            result_number = "1";
        }

        private bool _result_side_right;
        public bool result_side_right
        {
            get { return _result_side_right; }
            set
            {
                _result_side_right = value;
                OnPropertyChanged(nameof(result_side_right));
            }
        }

        private string _result_number;
        public string result_number
        {
            get { return _result_number; }
            set
            {
                _result_number = value;
                OnPropertyChanged(nameof(result_number));
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
                case "result_number":
                    error = Validate_result_number();
                    break;
            }
            return error;
        }

        private string Validate_result_number()
        {
            if (string.IsNullOrEmpty(result_number))
                return "Input is empty";
            else
            {
                if (int.TryParse(result_number, out int y))
                {
                    if (y < 0)
                        return "From 0";
                }
                else
                    return "Invalid input";
            }
            return null;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Stairs_Steps_Enumerate_Data GetInformation()
        {
            // Information gathered from window
            var information = new Stairs_Steps_Enumerate_Data();
            information.result_side_right = result_side_right;
            information.result_number = result_number;
            return information;
        }
    }
}
