using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "StairsStepsEnumerate"
    /// </summary>
    public class StairsStepsEnumerateViewModel : BaseViewModel
    {
        private const int _startNumberMinValue = 0;

        #region PROPERTIES

        private StairsStepsEnumerateModel _model;
        public StairsStepsEnumerateModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private int _startNumber;
        public int StartNumber
        {
            get { return _startNumber; }
            set
            {
                _startNumber = value;
                OnPropertyChanged(nameof(StartNumber));
            }
        }

        private string _startNumberString;
        public string StartNumberString
        {
            get { return _startNumberString; }
            set
            {
                _startNumberString = value;
                OnPropertyChanged(nameof(StartNumberString));
            }
        }

        private bool _sideRight;
        public bool SideRight
        {
            get { return _sideRight; }
            set
            {
                _sideRight = value;
                OnPropertyChanged(nameof(SideRight));
            }
        }

        #endregion

        public StairsStepsEnumerateViewModel()
        {
            SideRight = true;
            StartNumber = 1;
            StartNumberString = StartNumber.ToString();

            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

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
                case "StartNumberString":
                    error = ValidateInputIsWholeNumber(out int number, StartNumberString);
                    if (string.IsNullOrEmpty(error))
                    {
                        StartNumber = number;
                        error = ValidateResultNumber();
                    }
                    break;
            }
            return error;
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

        private string ValidateResultNumber()
        {
            if (StartNumber < _startNumberMinValue)
                return $"From {_startNumberMinValue}";
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model = BaseModel as StairsStepsEnumerateModel;

            Model.StartNumber = StartNumber;
            Model.SideRight = SideRight;

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