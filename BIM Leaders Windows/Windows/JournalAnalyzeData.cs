using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Journal Analyze"
    /// </summary>
    public class JournalAnalyzeData : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="JournalAnalyzeData"/> class.
        /// </summary>
        public JournalAnalyzeData(DataSet commandsDataSet)
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
