using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "Purge"
    /// </summary>
    public class PurgeViewModel : BaseViewModel
    {
        #region PROPERTIES
        
        private PurgeModel _model;
        public PurgeModel Model
        {
            get { return _model; }
            set { _model = value; }
        }
        
        private bool _purgeRooms;
        public bool PurgeRooms
        {
            get { return _purgeRooms; }
            set
            {
                _purgeRooms = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeTags;
        public bool PurgeTags
        {
            get { return _purgeTags; }
            set
            {
                _purgeTags = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeFilters;
        public bool PurgeFilters
        {
            get { return _purgeFilters; }
            set
            {
                _purgeFilters = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeViewTemplates;
        public bool PurgeViewTemplates
        {
            get { return _purgeViewTemplates; }
            set
            {
                _purgeViewTemplates = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeSheets;
        public bool PurgeSheets
        {
            get { return _purgeSheets; }
            set
            {
                _purgeSheets = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeLineStyles;
        public bool PurgeLineStyles
        {
            get { return _purgeLineStyles; }
            set
            {
                _purgeLineStyles = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private bool _purgeLinePatterns;
        public bool PurgeLinePatterns
        {
            get { return _purgeLinePatterns; }
            set
            {
                _purgeLinePatterns = value;
                OnPropertyChanged(nameof(PurgeRooms));
                OnPropertyChanged(nameof(PurgeTags));
                OnPropertyChanged(nameof(PurgeFilters));
                OnPropertyChanged(nameof(PurgeViewTemplates));
                OnPropertyChanged(nameof(PurgeSheets));
                OnPropertyChanged(nameof(PurgeLineStyles));
                OnPropertyChanged(nameof(PurgeLinePatterns));
            }
        }

        private string _linePatternName;
        public string LinePatternName
        {
            get { return _linePatternName; }
            set
            {
                _linePatternName = value;
                OnPropertyChanged(nameof(LinePatternName));
            }
        }

        #endregion

        public PurgeViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (PurgeModel)BaseModel;

            PurgeRooms = true;
            PurgeTags = true;
            PurgeFilters = true;
            PurgeViewTemplates = true;
            PurgeSheets = true;
            PurgeLineStyles = true;
            PurgeLinePatterns = true;
            LinePatternName = "IMPORT";
        }

        #endregion

        #region VALIDATION

        public string Error { get { return null; } }

        public string this[string propertyName]
        {
            get
            {
                return GetValidationError(propertyName);
            }
        }

        string GetValidationError(string propertyName)
        {
            string error = null;
            
            switch (propertyName)
            {
                case "PurgeRooms":
                    error = ValidateResult(); break;
                case "PurgeTags":
                    error = ValidateResult(); break;
                case "PurgeFilters":
                    error = ValidateResult(); break;
                case "PurgeViewTemplates":
                    error = ValidateResult(); break;
                case "PurgeSheets":
                    error = ValidateResult(); break;
                case "PurgeLineStyles":
                    error = ValidateResult(); break;
                case "PurgeLinePatterns":
                    error = ValidateResult(); break;
                case "LinePatternName":
                    error = ValidatePatternName(); break;
            }
            return error;
        }

        private string ValidateResult()
        {
            if (PurgeRooms == false
                && PurgeTags == false
                && PurgeFilters == false
                && PurgeViewTemplates == false
                && PurgeSheets == false
                && PurgeLineStyles == false
                && PurgeLinePatterns == false)
                return "Check at least one purge item";
            return null;
        }

        private string ValidatePatternName()
        {
            if (string.IsNullOrEmpty(LinePatternName))
                return "Input is empty";
            else
            {
                if (LinePatternName.Length < 3)
                    return "From 3 symbols";
            }
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.PurgeRooms = PurgeRooms;
            Model.PurgeTags = PurgeTags;
            Model.PurgeFilters = PurgeFilters;
            Model.PurgeViewTemplates = PurgeViewTemplates;
            Model.PurgeSheets = PurgeSheets;
            Model.PurgeLineStyles = PurgeLineStyles;
            Model.PurgeLinePatterns = PurgeLinePatterns;
            Model.LinePatternName = LinePatternName;

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