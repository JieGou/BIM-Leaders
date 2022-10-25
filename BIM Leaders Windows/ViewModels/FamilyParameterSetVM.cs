using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "FamilyParameterSet"
    /// </summary>
    public class FamilyParameterSetVM : INotifyPropertyChanged, IDataErrorInfo
    {
        #region PROPERTIES

        private FamilyParameterSetM _model;
        public FamilyParameterSetM Model
        {
            get { return _model; }
            set { _model = value; }
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

        #endregion

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="FamilyParameterSetVM"/> class.
        /// </summary>
        public FamilyParameterSetVM(FamilyParameterSetM model)
        {
            Model = model;

            RunCommand = new RunCommand(RunAction);
        }

        #region VALIDATION

        public string Error { get { return null; } }

        public string this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

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

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.SelectedParameterName = ParametersListSelected;
            Model.Value = ParameterValue;

            Model.Run();
        }

        #endregion
    }
}
