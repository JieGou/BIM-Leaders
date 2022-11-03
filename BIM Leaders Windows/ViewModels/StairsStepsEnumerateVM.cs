using System;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "StairsStepsEnumerate"
    /// </summary>
    public class StairsStepsEnumerateVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _startNumberMinValue = 0;

        #region PROPERTIES

        private StairsStepsEnumerateM _model;
        public StairsStepsEnumerateM Model
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

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="StairsStepsEnumerateVM"/> class.
        /// </summary>
        public StairsStepsEnumerateVM(StairsStepsEnumerateM model)
        {
            Model = model;

            SideRight = true;
            StartNumber = 1;
            StartNumberString = StartNumber.ToString();

            RunCommand = new RunCommand(RunAction);
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

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.StartNumber = StartNumber;
            Model.SideRight = SideRight;

            Model.Run();

            CloseAction();
        }

        public Action CloseAction { get; set; }

        #endregion

    }
}
