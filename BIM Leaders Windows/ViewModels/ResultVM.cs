using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model "Result" windows
    /// </summary>
    public class ResultVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        public bool Closed { get; private set; }

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

        public ResultVM(string commandName, string reportText)
        {
            CommandName = commandName;
            ReportText = reportText;

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
            {
                Closed = true;
                window.Close();
            }
        }

        #endregion
    }
}