using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Walls_Compare"/>
    /// </summary>
    public class Walls_Compare_Data : INotifyPropertyChanged
    {
        UIDocument uidoc;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Walls_Compare_Data"/> class.
        /// </summary>

        public Walls_Compare_Data()
        {
            
        }

        /// <summary>
        /// Populates the fill types list.
        /// </summary>
        public List<string> Create_Fill_List(UIDocument uidoc)
        {
            var doc = uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<FilledRegionType> fill_types_all = collector.OfClass(typeof(FilledRegionType))
                .Cast<FilledRegionType>(); //LINQ function;

            // Get unique fills names list
            List<FilledRegionType> fill_types = new List<FilledRegionType>();
            List<string> fill_types_names = new List<string>();
            foreach (FilledRegionType i in fill_types_all)
            {
                string fill_type_name = i.Name;
                if (!fill_types_names.Contains(fill_type_name))
                {
                    fill_types.Add(i);
                    fill_types_names.Add(fill_type_name);
                }
            }

            return fill_types_names;
        }

        private List<string> _fill_types_names;
        public List<string> fill_types_names
        {
            get { return _fill_types_names; }
            set
            {
                var doc = uidoc;
                _fill_types_names = Create_Fill_List(doc);
                OnPropertyChanged(nameof(fill_types_names));
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


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Walls_Compare_Data GetInformation()
        {
            // Information gathered from window
            var information = new Walls_Compare_Data();
            /*
            information.result_spots = result_spots;
            information.result_thickness = result_thickness;
            */
            return information;
        }
    }
}
