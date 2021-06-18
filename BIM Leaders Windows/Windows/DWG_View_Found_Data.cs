using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_View_Find"
    /// </summary>
    public class DWG_View_Found_Data : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DWG_View_Found_Data"/> class.
        /// </summary>
        public DWG_View_Found_Data(DataSet dwgDataSet)
        {
            this._dwg = dwgDataSet;
        }

        private DataSet _dwg;
        public DataSet dwg
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
