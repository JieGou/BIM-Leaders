using System.Collections.Generic;
using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "FamilyParameterSet"
    /// </summary>
    public class FamilyParameterSetViewModel : BaseViewModel
    {
        #region PROPERTIES

        private FamilyParameterSetModel _model;
        public FamilyParameterSetModel Model
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

        public FamilyParameterSetViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
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

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model = BaseModel as FamilyParameterSetModel;

            Model.SelectedParameterName = ParametersListSelected;
            Model.Value = ParameterValue;

            Model.Run();

            CloseAction(window);
        }

        private protected override void CloseAction(Window window)
        {
            if (window != null)
            {
                Closed = true;
                window.Close();
            }
        }

        #endregion
    }
}