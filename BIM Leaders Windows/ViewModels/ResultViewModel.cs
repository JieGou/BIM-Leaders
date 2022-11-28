using System.ComponentModel;
using System.Windows;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model "Result" windows
    /// </summary>
    public class ResultViewModel : BaseViewModel
    {
        #region PROPERTIES

        private string _commandName;
        public string CommandName
        {
            get { return _commandName; }
            set
            {
                _commandName = value;
                OnPropertyChanged(nameof(CommandName));
            }
        }

        private string _reportText;
        public string ReportText
        {
            get { return _reportText; }
            set
            {
                _reportText = value;
                OnPropertyChanged(nameof(ReportText));
            }
        }

        #endregion

        public ResultViewModel(string commandName, string reportText)
        {
            CommandName = commandName;
            ReportText = reportText;

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