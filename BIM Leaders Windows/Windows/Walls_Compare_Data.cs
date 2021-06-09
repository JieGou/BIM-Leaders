using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Runtime.CompilerServices;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Walls_Compare"/>
    /// </summary>
    public class Walls_Compare_Data : INotifyPropertyChanged
    {
        UIDocument uidoc = null;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Walls_Compare_Data"/> class.
        /// </summary>

        public Walls_Compare_Data(UIDocument uidoc)
        {
            this.uidoc = uidoc;
        }

        /// <summary>
        /// Populates the materials list.
        /// </summary>
        public SortedDictionary<string, ElementId> Create_Material_List()
        {
            var doc = uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<Material> materials_all = collector.OfClass(typeof(Material)).OrderBy(a => a.Name)
                .Cast<Material>(); //LINQ function;

            // Get unique fills names list
            List<Material> materials = new List<Material>();
            List<string> materials_names = new List<string>();
            foreach (Material i in materials_all)
            {
                string material_name = i.Name;
                if (!materials_names.Contains(material_name))
                {
                    materials.Add(i);
                    materials_names.Add(material_name);
                }
            }

            SortedDictionary<string, ElementId> materials_list = new SortedDictionary<string, ElementId>();
            foreach (Material i in materials)
            {
                materials_list.Add(i.Name, i.Id);
            }
            /*
            SortedDictionary<ElementId, string> materials_list_sorted = new SortedDictionary<ElementId, string>();

            materials_list.ToList().Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            foreach (var value in materials_list)
            {
                materials_list_sorted.Add(value.Key, value.Value);
            }
            */
            return materials_list;
        }

        /// <summary>
        /// Populates the fill types list.
        /// </summary>
        public SortedDictionary<string, ElementId> Create_Fill_List()
        {
            var doc = uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<FilledRegionType> fill_types_all = collector.OfClass(typeof(FilledRegionType)).OrderBy(a => a.Name)
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

            //List<KeyValuePair<string, ElementId>> list = new List<KeyValuePair<string, ElementId>>();
            SortedDictionary<string, ElementId> fill_types_list = new SortedDictionary<string, ElementId>();
            foreach (FilledRegionType i in fill_types)
            {
                fill_types_list.Add(i.Name, i.Id);
            }

            return fill_types_list;
        }

        public SortedDictionary<string, ElementId> mats_list
        {
            get { return Create_Material_List(); }
        }
        public SortedDictionary<string, ElementId> fill_types_list
        {
            get { return Create_Fill_List(); }
        }

        private ElementId _mats_list_sel;
        public ElementId mats_list_sel 
        {
            get { return _mats_list_sel; }
            set
            {
                _mats_list_sel = value;
                OnPropertyChanged(nameof(mats_list_sel));
            }
        }

        private ElementId _fill_types_list_sel;
        public ElementId fill_types_list_sel
        {
            get { return _fill_types_list_sel; }
            set
            {
                _fill_types_list_sel = value;
                OnPropertyChanged(nameof(fill_types_list_sel));
            }
        }


        private List<KeyValuePair<string, ElementId>> material;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
