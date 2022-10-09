using System.Collections.Generic;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class FamilyParameterSetVM : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="FamilyParameterSetVM"/> class.
        /// </summary>
        public FamilyParameterSetVM(List<string> parametersList)
        {
            ParametersList = parametersList;
        }

        private List<string> _parametersList;
        public List<string> ParametersList
        {
            get { return _parametersList; }
            set { _parametersList = value; }
        }

        private string _parametersListSelected;
        public string ParametersListSelected 
        {
            get { return _parametersListSelected; }
            set
            {
                _parametersListSelected = value;
                OnPropertyChanged(nameof(ParametersListSelected));
            }
        }

        private string _parameterValue;
        public string ParameterValue
        {
            get { return _parameterValue; }
            set
            {
                _parameterValue = value;
                OnPropertyChanged(nameof(ParameterValue));
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

        public string Error { get { return null; } }

        string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "ParameterValue":
                    error = ValidateParameterValue();
                    break;
            }
            return error;
        }

        private string ValidateParameterValue()
        {
            if (string.IsNullOrEmpty(ParameterValue))
                return "Input is empty";
            else
            {
                if (ParameterValue.StartsWith("="))
                    return "Cannot set a formula";
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
