using System.Data;
using System.Windows;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "JournalAnalyze"
    /// </summary>
    public class JournalAnalyzeViewModel : BaseViewModel
    {
        #region PROPERTIES

        private DataSet _commandsDataSet;
        public DataSet CommandsDataSet
        {
            get { return _commandsDataSet; }
            set { _commandsDataSet = value; }
        }

        #endregion

        public JournalAnalyzeViewModel(DataSet commandsDataSet)
        {
            CommandsDataSet = commandsDataSet;

            CloseCommand = new CommandWindow(CloseAction);
        }

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