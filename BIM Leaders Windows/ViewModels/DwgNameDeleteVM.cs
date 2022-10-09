using System.Collections.Generic;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_Name_Delete"
    /// </summary>
    public class DwgNameDeleteVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DwgNameDeleteVM"/> class.
        /// </summary>
        public DwgNameDeleteVM(SortedDictionary<string, int> dwgList)
        {
            DwgList = dwgList;
        }

        private SortedDictionary<string, int> _dwgList = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> DwgList
        {
            get { return _dwgList; }
            set { _dwgList = value; }
        }

        private int _dwgListSelected;
        public int DwgListSelected 
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
