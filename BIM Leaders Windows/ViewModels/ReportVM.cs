using System.Data;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "Checker"
    /// </summary>
    public class ReportVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private DataSet _report;
        public DataSet Report
        {
            get { return _report; }
            set { _report = value; }
        }

        #endregion

        public ReportVM(DataSet reportDataSet)
        {
            Report = reportDataSet;

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