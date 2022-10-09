using System.Collections.Generic;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Walls_Compare"
    /// </summary>
    public class WallsCompareVM : INotifyPropertyChanged
    {
        public string Error { get { return null; } }

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsCompareVM"/> class.
        /// </summary>
        public WallsCompareVM(SortedDictionary<string, int> listMaterials, SortedDictionary<string, int> listFillTypes)
        {
            ResultLinks = true;
            ListMaterials = listMaterials;
            ListFillTypes = listFillTypes;
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

        private SortedDictionary<string, int> _listMaterials;
        public SortedDictionary<string, int> ListMaterials
        {
            get { return _listMaterials; }
            set { _listMaterials = value; }
        }

        private SortedDictionary<string, int> _listFillTypes;
        public SortedDictionary<string, int> ListFillTypes
        {
            get { return _listFillTypes; }
            set { _listFillTypes = value; }
        }

        private int _listMaterialsSelected;
        public int ListMaterialsSelected 
        {
            get { return _listMaterialsSelected; }
            set
            {
                _listMaterialsSelected = value;
                OnPropertyChanged(nameof(ListMaterialsSelected));
            }
        }

        private int _listFillTypesSelected;
        public int ListFillTypesSelected
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
