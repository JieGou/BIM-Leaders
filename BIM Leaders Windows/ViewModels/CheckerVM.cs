using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "Checker"
    /// </summary>
    public class CheckerVM : INotifyPropertyChanged, IDataErrorInfo
    {
        // Minimal height that can be accepted.
        private const int _stairsHeadHeightMinValue = 200;

        #region PROPERTIES

        private CheckerM _model;
        public CheckerM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public bool Closed { get; private set; }

        private string _prefix;
        public string Prefix
        {
            get { return _prefix; }
            set
            {
                _prefix = value;
                OnPropertyChanged(nameof(Prefix));
            }
        }

        private List<bool> _checkCategories { get; set; }
        public List<bool> CheckCategories
        {
            get { return _checkCategories; }
            set
            {
                _checkCategories = value;
                OnPropertyChanged(nameof(CheckCategories));
            }
        }

        private List<bool> _checkModel { get; set; }
        public List<bool> CheckModel
        {
            get { return _checkModel; }
            set
            {
                _checkModel = value;
                OnPropertyChanged(nameof(CheckModel));
            }
        }

        private List<bool> _checkCodes { get; set; }
        public List<bool> CheckCodes
        {
            get { return _checkCodes; }
            set
            {
                _checkCodes = value;
                OnPropertyChanged(nameof(CheckCodes));
            }
        }

        private string _stairsHeadHeightString { get; set; }
        public string StairsHeadHeightString
        {
            get { return _stairsHeadHeightString; }
            set
            {
                _stairsHeadHeightString = value;
                OnPropertyChanged(nameof(StairsHeadHeightString));
            }
        }

        private int _stairsHeadHeight { get; set; }
        public int StairsHeadHeight
        {
            get { return _stairsHeadHeight; }
            set { _stairsHeadHeight = value; }
        }

        #endregion

        public CheckerVM(CheckerM model)
        {
            Model = model;

            Prefix = "PRE_";
            CheckCategories = Enumerable.Repeat(false, 24).ToList();
            CheckCategories[6] = true;
            CheckModel = Enumerable.Repeat(false, 14).ToList();
            CheckCodes = Enumerable.Repeat(false, 2).ToList();
            StairsHeadHeight = 210;
            StairsHeadHeightString = StairsHeadHeight.ToString();

            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                case "Prefix":
                    error = ValidateResultPrefix();
                    break;
                case "InputHeight":
                    error = ValidateInputIsWholeNumber(out int height, StairsHeadHeightString);
                    if (string.IsNullOrEmpty(error))
                    {
                        StairsHeadHeight = height;
                        error = ValidateResultHeight();
                    }
                    break;
                default:
                    break;
            }
            return error;
        }

        private string ValidateResultPrefix()
        {
            if (string.IsNullOrEmpty(Prefix))
                return "Input is empty";
            else
            {
                if (Prefix.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateResultCheckboxes()
        {
            if (!CheckCategories.Contains(true) && !CheckModel.Contains(true) && !CheckCodes.Contains(true))
                return "Check at least one item";
            return null;
        }

        private string ValidateInputIsWholeNumber(out int numberParsed, string number)
        {
            numberParsed = 0;

            if (string.IsNullOrEmpty(number))
                return "Input is empty";
            if (!int.TryParse(number, out numberParsed))
                return "Not a whole number";

            return null;
        }

        private string ValidateResultHeight()
        {
            if (StairsHeadHeight < _stairsHeadHeightMinValue)
                return $"Must be over {_stairsHeadHeightMinValue} cm";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.CheckCategories = CheckCategories;
            Model.Prefix = Prefix;
            Model.CheckModel = CheckModel;
            Model.CheckCodes = CheckCodes;
            Model.StairsHeadHeight = StairsHeadHeight;

            Model.Run();

            CloseAction(window);
        }

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