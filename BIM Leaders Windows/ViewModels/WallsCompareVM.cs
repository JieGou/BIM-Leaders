using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Walls_Compare"
    /// </summary>
    public class WallsCompareVM : INotifyPropertyChanged
    {
        UIDocument Uidoc = null;
        public string Error { get { return null; } }
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsCompareVM"/> class.
        /// </summary>

        public WallsCompareVM(UIDocument uidoc)
        {
            Uidoc = uidoc;
            ResultLinks = true;
        }

        /// <summary>
        /// Gets or sets a value indicating picked side for <see cref="LevelsAlignVM"/> annotations.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if side 1 is chosen, if side 2 is chosen, then <c>false</c>.
        /// </value>
        private bool _resultLinks;
        public bool ResultLinks
        {
            get { return _resultLinks; }
            set
            {
                _resultLinks = value;
                OnPropertyChanged(nameof(ResultLinks));
            }
        }

        /// <summary>
        /// Populates the materials list.
        /// </summary>
        public SortedDictionary<string, ElementId> CreateListMaterials()
        {
            var doc = Uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<Material> materialsAll = collector.OfClass(typeof(Material)).OrderBy(a => a.Name)
                .Cast<Material>(); //LINQ function;

            // Get unique fills names list
            List<Material> materials = new List<Material>();
            List<string> materialsNames = new List<string>();
            foreach (Material i in materialsAll)
            {
                string materialName = i.Name;
                if (!materialsNames.Contains(materialName))
                {
                    materials.Add(i);
                    materialsNames.Add(materialName);
                }
            }

            SortedDictionary<string, ElementId> materialsList = new SortedDictionary<string, ElementId>();
            foreach (Material i in materials)
            {
                materialsList.Add(i.Name, i.Id);
            }

            return materialsList;
        }

        /// <summary>
        /// Populates the fill types list.
        /// </summary>
        public SortedDictionary<string, ElementId> CreateListFillTypes()
        {
            var doc = Uidoc.Document;

            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<FilledRegionType> fillTypesAll = collector.OfClass(typeof(FilledRegionType)).OrderBy(a => a.Name)
                .Cast<FilledRegionType>(); //LINQ function;

            // Get unique fills names list
            List<FilledRegionType> fillTypes = new List<FilledRegionType>();
            List<string> fillTypesNames = new List<string>();
            foreach (FilledRegionType i in fillTypesAll)
            {
                string fillTypeName = i.Name;
                if (!fillTypesNames.Contains(fillTypeName))
                {
                    fillTypes.Add(i);
                    fillTypesNames.Add(fillTypeName);
                }
            }

            //List<KeyValuePair<string, ElementId>> list = new List<KeyValuePair<string, ElementId>>();
            SortedDictionary<string, ElementId> fillTypesList = new SortedDictionary<string, ElementId>();
            foreach (FilledRegionType i in fillTypes)
            {
                fillTypesList.Add(i.Name, i.Id);
            }

            return fillTypesList;
        }

        public SortedDictionary<string, ElementId> ListMaterials
        {
            get { return CreateListMaterials(); }
        }
        public SortedDictionary<string, ElementId> ListFillTypes
        {
            get { return CreateListFillTypes(); }
        }

        private ElementId _listMaterialsSelected;
        public ElementId ListMaterialsSelected 
        {
            get { return _listMaterialsSelected; }
            set
            {
                _listMaterialsSelected = value;
                OnPropertyChanged(nameof(ListMaterialsSelected));
            }
        }

        private ElementId _listFillTypesSelected;
        public ElementId ListFillTypesSelected
        {
            get { return _listFillTypesSelected; }
            set
            {
                _listFillTypesSelected = value;
                OnPropertyChanged(nameof(ListFillTypesSelected));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
