using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_View_Find"
    /// </summary>
    public class CheckerReportData : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="CheckerReportData"/> class.
        /// </summary>
        public CheckerReportData(DataSet reportDataSet)
        {
            this._checkReport = reportDataSet;
        }

        private DataSet _checkReport;
        public DataSet CheckReport
        {
            get { return _checkReport; }
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
