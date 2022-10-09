using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class DwgNameDeleteVM : INotifyPropertyChanged
    {
        private Document doc = null;

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DwgNameDeleteVM"/> class.
        /// </summary>
        public DwgNameDeleteVM(Document doc)
        {
            this.doc = doc;
        }

        /// <summary>
        /// Populates the DWG list.
        /// </summary>
        public SortedDictionary<string, ElementId> createDwgList()
        {
            // Get DWGs
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<ImportInstance> dwgTypesAll = collector.OfClass(typeof(ImportInstance)).OrderBy(a => a.Name)
                .Cast<ImportInstance>(); //LINQ function;

            // Get unique imports names list
            List<ImportInstance> dwgTypes = new List<ImportInstance>();
            List<string> dwgTypesNames = new List<string>();
            foreach (ImportInstance i in dwgTypesAll)
            {
                string dwgTypeName = i.Category.Name;
                if (!dwgTypesNames.Contains(dwgTypeName))
                {
                    dwgTypes.Add(i);
                    dwgTypesNames.Add(dwgTypeName);
                }
            }

            SortedDictionary<string, ElementId> dwgTypesList = new SortedDictionary<string, ElementId>();
            foreach (ImportInstance i in dwgTypes)
            {
                dwgTypesList.Add(i.Category.Name, i.Id);
            }

            return dwgTypesList;
        }


        public SortedDictionary<string, ElementId> DwgList
        {
            get { return createDwgList(); }
        }

        private ElementId _dwgListSelected;
        public ElementId DwgListSelected 
        {
            get { return _dwgListSelected; }
            set
            {
                _dwgListSelected = value;
                OnPropertyChanged(nameof(DwgListSelected));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
