using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class FamilyParameterSetData : INotifyPropertyChanged, IDataErrorInfo
    {
        UIDocument Uidoc = null;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="FamilyParameterSetData"/> class.
        /// </summary>

        public FamilyParameterSetData(UIDocument uidoc)
        {
            Uidoc = uidoc;
        }

        /// <summary>
        /// Populates the DWG list.
        /// </summary>
        public List<string> CreateParametersList()
        {
            Document doc = Uidoc.Document;

            // Get unique parameters
            IList<FamilyParameter> parametersNamesAll = doc.FamilyManager.GetParameters();
            List<FamilyParameter> parameters = new List<FamilyParameter>();
            List<string> parametersNames = new List<string>();
            foreach (FamilyParameter i in parametersNamesAll)
            {
                string parameterName = i.Definition.Name;
                if (!parametersNames.Contains(parameterName))
                {
                    parameters.Add(i);
                    parametersNames.Add(parameterName);
                }
            }
            /*
            SortedDictionary<string, ElementId> parameters_list = new SortedDictionary<string, ElementId>();
            foreach (FamilyParameter i in parameters)
            {
                parameters_list.Add(i.Definition.Name, i.Id);
            }
            */

            return parametersNames;
        }


        public List<string> ParametersList
        {
            get { return CreateParametersList(); }
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
