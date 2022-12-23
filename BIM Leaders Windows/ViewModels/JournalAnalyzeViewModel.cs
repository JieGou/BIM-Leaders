using BIM_Leaders_Logic;
using System.Data;
using System.Linq;
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

        private bool _analyzeCommands;
        public bool AnalyzeCommands
        {
            get { return _analyzeCommands; }
            set
            {
                _analyzeCommands = value;
                OnPropertyChanged(nameof(AnalyzeCommands));
            }
        }

        private bool _analyzeActivate;
        public bool AnalyzeActivate
        {
            get { return _analyzeActivate; }
            set
            {
                _analyzeActivate = value;
                OnPropertyChanged(nameof(AnalyzeActivate));
            }
        }

        private bool _analyzeWheel;
        public bool AnalyzeWheel
        {
            get { return _analyzeWheel; }
            set
            {
                _analyzeWheel = value;
                OnPropertyChanged(nameof(AnalyzeWheel));
            }
        }

        private bool _analyzeMouseButtons;
        public bool AnalyzeMouseButtons
        {
            get { return _analyzeMouseButtons; }
            set
            {
                _analyzeMouseButtons = value;
                OnPropertyChanged(nameof(AnalyzeMouseButtons));
            }
        }
        private bool _analyzeMouseMove;
        public bool AnalyzeMouseMove
        {
            get { return _analyzeMouseMove; }
            set
            {
                _analyzeMouseMove = value;
                OnPropertyChanged(nameof(AnalyzeMouseMove));
            }
        }

        private bool _analyzeKey;
        public bool AnalyzeKey
        {
            get { return _analyzeKey; }
            set
            {
                _analyzeKey = value;
                OnPropertyChanged(nameof(AnalyzeKey));
            }
        }

        private DataSet _journalDataSet;
        public DataSet JournalDataSet
        {
            get { return _journalDataSet; }
            set { _journalDataSet = value; }
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

            AnalyzeCommands = true;
            AnalyzeActivate = true;
            AnalyzeWheel = true;
            AnalyzeMouseButtons = true;
            AnalyzeMouseMove = true;
            AnalyzeKey = true;
        }

        #endregion

        #region VALIDATION

        private protected override string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "AnalyzeCommands":
                    error = ValidateResultCheckboxes();
                    break;
                case "AnalyzeActivate":
                    error = ValidateResultCheckboxes();
                    break;
                case "AnalyzeWheel":
                    error = ValidateResultCheckboxes();
                    break;
                case "AnalyzeMouseButtons":
                    error = ValidateResultCheckboxes();
                    break;
                case "AnalyzeMouseMove":
                    error = ValidateResultCheckboxes();
                    break;
                case "AnalyzeKey":
                    error = ValidateResultCheckboxes();
                    break;
                default:
                    break;
            }
            return error;
        }

        private string ValidateResultCheckboxes()
        {
            if (!AnalyzeCommands && 
                !AnalyzeActivate &&
                !AnalyzeWheel &&
                !AnalyzeMouseButtons &&
                !AnalyzeMouseMove &&
                !AnalyzeKey
                )
                return "Check at least one item";
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.AnalyzeCommands = AnalyzeCommands;
            Model.AnalyzeActivate = AnalyzeActivate;
            Model.AnalyzeWheel = AnalyzeWheel;
            Model.AnalyzeMouseButtons = AnalyzeMouseButtons;
            Model.AnalyzeMouseMove = AnalyzeMouseMove;
            Model.AnalyzeKey = AnalyzeKey;

            Model.Run();

            CloseAction(window);
        }

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