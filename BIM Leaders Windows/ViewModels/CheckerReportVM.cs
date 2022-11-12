using System.Data;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "Checker"
    /// </summary>
    public class CheckerReportVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private DataSet _checkReport;
        public DataSet CheckReport
        {
            get { return _checkReport; }
            set { _checkReport = value; }
        }

        #endregion

        public CheckerReportVM(DataSet reportDataSet)
        {
            CheckReport = reportDataSet;

            CloseCommand = new CommandWindow(CloseAction);
        }

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region COMMANDS

        public ICommand CloseCommand { get; set; }

        private void CloseAction(Window window)
        {
            if (window != null)
                window.Close();
        }

        #endregion
    }
}