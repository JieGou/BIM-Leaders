using BIM_Leaders_Logic;
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

        private JournalAnalyzeModel _model;
        public JournalAnalyzeModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private DataSet _commandsDataSet;
        public DataSet CommandsDataSet
        {
            get { return _commandsDataSet; }
            set { _commandsDataSet = value; }
        }

        #endregion

        public JournalAnalyzeViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (JournalAnalyzeModel)BaseModel;

            CommandsDataSet = Model.Get;
        }

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