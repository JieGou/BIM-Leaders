using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class DWG_Name_Delete_Data : INotifyPropertyChanged
    {
        UIDocument uidoc = null;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DWG_Name_Delete_Data"/> class.
        /// </summary>

        public DWG_Name_Delete_Data(UIDocument uidoc)
        {
            this.uidoc = uidoc;
        }

        /// <summary>
        /// Populates the DWG list.
        /// </summary>
        public SortedDictionary<string, ElementId> Create_DWG_List()
        {
            var doc = uidoc.Document;

            // Get DWGs
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<ImportInstance> dwg_types_all = collector.OfClass(typeof(ImportInstance)).OrderBy(a => a.Name)
                .Cast<ImportInstance>(); //LINQ function;

            // Get unique imports names list
            List<ImportInstance> dwg_types = new List<ImportInstance>();
            List<string> dwg_types_names = new List<string>();
            foreach (ImportInstance i in dwg_types_all)
            {
                string dwg_type_name = i.Category.Name;
                if (!dwg_types_names.Contains(dwg_type_name))
                {
                    dwg_types.Add(i);
                    dwg_types_names.Add(dwg_type_name);
                }
            }

            SortedDictionary<string, ElementId> dwg_types_list = new SortedDictionary<string, ElementId>();
            foreach (ImportInstance i in dwg_types)
            {
                dwg_types_list.Add(i.Category.Name, i.Id);
            }

            return dwg_types_list;
        }


        public SortedDictionary<string, ElementId> dwg_list
        {
            get { return Create_DWG_List(); }
        }

        private ElementId _dwg_list_sel;
        public ElementId dwg_list_sel 
        {
            get { return _dwg_list_sel; }
            set
            {
                _dwg_list_sel = value;
                OnPropertyChanged(nameof(dwg_list_sel));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
