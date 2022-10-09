using System.Data;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Journal Analyze"
    /// </summary>
    public class JournalAnalyzeVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="JournalAnalyzeVM"/> class.
        /// </summary>
        public JournalAnalyzeVM(DataSet commandsDataSet)
        {
            _commandsDataSet = commandsDataSet;
        }

        private DataSet _commandsDataSet;
        public DataSet CommandsDataSet
        {
            get { return _commandsDataSet; }
            set { }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
