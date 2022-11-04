using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model "Report" windows
    /// </summary>
    public class ReportVM : INotifyPropertyChanged
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

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="StairsStepsEnumerateVM"/> class.
        /// </summary>
        public ReportVM(string commandName, string reportText)
        {
            CommandName = commandName;
            ReportText = reportText;
        }

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}