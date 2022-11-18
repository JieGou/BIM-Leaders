using System.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "JournalAnalyze"
    /// </summary>
    public class JournalAnalyzeVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        public bool Closed { get; private set; }

        private DataSet _commandsDataSet;
        public DataSet CommandsDataSet
        {
            get { return _commandsDataSet; }
            set { _commandsDataSet = value; }
        }

        #endregion

        public JournalAnalyzeVM(DataSet commandsDataSet)
        {
            CommandsDataSet = commandsDataSet;

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