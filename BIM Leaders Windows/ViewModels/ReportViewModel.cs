using System.Data;
using System.Windows;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "Checker"
    /// </summary>
    public class ReportViewModel : BaseViewModel
    {
        #region PROPERTIES

        private DataSet _report;
        public DataSet Report
        {
            get { return _report; }
            set { _report = value; }
        }

        #endregion

        public ReportViewModel(DataSet reportDataSet)
        {
            Report = reportDataSet;

            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData() { }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window) { }

        private protected override void CloseAction(Window window)
        {
            if (window != null)
            {
                Closed = true;
                window.Close();
            }
        }

        #endregion
    }
}