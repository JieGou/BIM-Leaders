﻿using System.Data;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_View_Find"
    /// </summary>
    public class DwgViewFoundVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DwgViewFoundVM"/> class.
        /// </summary>
        public DwgViewFoundVM(DataSet dwgDataSet)
        {
            _dwg = dwgDataSet;
        }

        private DataSet _dwg;
        public DataSet Dwg
        {
            get { return _dwg; }
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}