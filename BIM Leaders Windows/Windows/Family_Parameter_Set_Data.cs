using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class Family_Parameter_Set_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        UIDocument uidoc = null;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Family_Parameter_Set_Data"/> class.
        /// </summary>

        public Family_Parameter_Set_Data(UIDocument uidoc)
        {
            this.uidoc = uidoc;
        }

        /// <summary>
        /// Populates the DWG list.
        /// </summary>
        public List<string> Create_Parameters_List()
        {
            Document doc = uidoc.Document;

            // Get unique parameters
            IList<FamilyParameter> par_names_all = doc.FamilyManager.GetParameters();
            List<FamilyParameter> parameters = new List<FamilyParameter>();
            List<string> par_names = new List<string>();
            foreach (FamilyParameter i in par_names_all)
            {
                string par_name = i.Definition.Name;
                if (!par_names.Contains(par_name))
                {
                    parameters.Add(i);
                    par_names.Add(par_name);
                }
            }
            /*
            SortedDictionary<string, ElementId> parameters_list = new SortedDictionary<string, ElementId>();
            foreach (FamilyParameter i in parameters)
            {
                parameters_list.Add(i.Definition.Name, i.Id);
            }
            */

            return par_names;
        }


        public List<string> par_list
        {
            get { return Create_Parameters_List(); }
        }

        private string _par_list_sel;
        public string par_list_sel 
        {
            get { return _par_list_sel; }
            set
            {
                _par_list_sel = value;
                OnPropertyChanged(nameof(par_list_sel));
            }
        }

        private string _par_value;
        public string par_value
        {
            get { return _par_value; }
            set
            {
                _par_value = value;
                OnPropertyChanged(nameof(par_value));
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
                case "par_value":
                    error = Validate_par_value();
                    break;
            }
            return error;
        }

        private string Validate_par_value()
        {
            if (string.IsNullOrEmpty(par_value))
                return "Input is empty";
            else
            {
                if (par_value.StartsWith("="))
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
