﻿using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Dimension_Section_Floors"/>
    /// </summary>
    public class Dimension_Section_Floors_Data : INotifyPropertyChanged, IDataErrorInfo
    {
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Dimension_Section_Floors_Data"/> class.
        /// </summary>
        public Dimension_Section_Floors_Data()
        {
            result_spots = true;
            result_thickness = "10";
        }

        private bool _result_spots;
        public bool result_spots
        {
            get { return _result_spots; }
            set
            {
                _result_spots = value;
                OnPropertyChanged(nameof(result_spots));
            }
        }

        private string _result_thickness;
        public string result_thickness
        {
            get { return _result_thickness; }
            set
            {
                _result_thickness = value;
                OnPropertyChanged(nameof(result_thickness));
            }
        }
        /*
        // Boolean for abling/disabling the Ok button.
        private bool _IsValid;
        public bool IsValid
        {
            get { return _IsValid; }
            set
            {
                _IsValid = Validate_IsValid();
                OnPropertyChanged(nameof(IsValid));
            }
        }
        */

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
                case "result_thickness":
                    error = Validate_result_thickness();
                    break;
            }
            return error;
        }

        private string Validate_result_thickness()
        {
            if (string.IsNullOrEmpty(result_thickness))
                return "Input is empty";
            else
            {
                if (int.TryParse(result_thickness, out int y))
                {
                    if (y < 1 || y > 100)
                        return "From 1 to 100 cm";
                }
                else
                    return "Invalid input";
            }
            return null;
        }
        /*
        static readonly string[] ValidatedProperties =
{
            "result_thickness"
        };
        // Changing boolean for abling/disabling the Ok button.
        public bool Validate_IsValid()
        {
            foreach (string property in ValidatedProperties)
                if (GetValidationError(property) != null)
                    return true;
            return false;
        }
        */
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
        public Dimension_Section_Floors_Data GetInformation()
        {
            // Information gathered from window
            var information = new Dimension_Section_Floors_Data();
            information.result_spots = result_spots;
            information.result_thickness = result_thickness;
            return information;
        }
    }
}
