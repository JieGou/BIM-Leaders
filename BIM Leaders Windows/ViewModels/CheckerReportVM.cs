using System.Data;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "DWG_View_Find"
    /// </summary>
    public class CheckerReportVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="CheckerReportVM"/> class.
        /// </summary>
        public CheckerReportVM(DataSet reportDataSet)
        {
            _checkReport = reportDataSet;
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
