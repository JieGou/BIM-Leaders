using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_View_Find"
    /// </summary>
    public class Checker_Report_Data : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Checker_Report_Data"/> class.
        /// </summary>
        public Checker_Report_Data(DataSet reportDataSet)
        {
            this._check_report = reportDataSet;
        }

        private DataSet _check_report;
        public DataSet check_report
        {
            get { return _check_report; }
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
