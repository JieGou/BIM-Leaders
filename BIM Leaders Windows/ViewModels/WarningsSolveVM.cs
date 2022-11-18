using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WarningsSolve"
    /// </summary>
    public class WarningsSolveVM : INotifyPropertyChanged, IDataErrorInfo
    {
        #region PROPERTIES

        private WarningsSolveM _model;
        public WarningsSolveM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public bool Closed { get; private set; }

        private bool _fixWarningsJoin;
        public bool FixWarningsJoin
        {
            get { return _fixWarningsJoin; }
            set
            {
                _fixWarningsJoin = value;
                OnPropertyChanged(nameof(FixWarningsJoin));
                OnPropertyChanged(nameof(FixWarningsWallsAttached));
                OnPropertyChanged(nameof(FixWarningsRoomNotEnclosed));
            }
        }

        private bool _fixWarningsWallsAttached;
        public bool FixWarningsWallsAttached
        {
            get { return _fixWarningsWallsAttached; }
            set
            {
                _fixWarningsWallsAttached = value;
                OnPropertyChanged(nameof(FixWarningsJoin));
                OnPropertyChanged(nameof(FixWarningsWallsAttached));
                OnPropertyChanged(nameof(FixWarningsRoomNotEnclosed));
            }
        }

        private bool _fixWarningsRoomNotEnclosed;
        public bool FixWarningsRoomNotEnclosed
        {
            get { return _fixWarningsRoomNotEnclosed; }
            set
            {
                _fixWarningsRoomNotEnclosed = value;
                OnPropertyChanged(nameof(FixWarningsJoin));
                OnPropertyChanged(nameof(FixWarningsWallsAttached));
                OnPropertyChanged(nameof(FixWarningsRoomNotEnclosed));
            }
        }

        #endregion

        public WarningsSolveVM(WarningsSolveM model)
        {
            Model = model;

            FixWarningsJoin = true;
            FixWarningsWallsAttached = false;
            FixWarningsRoomNotEnclosed = true;

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
                case "FixWarningsJoin":
                    error = ValidateResult();
                    break;
                case "FixWarningsWallsAttached":
                    error = ValidateResult();
                    break;
                case "FixWarningsRoomNotEnclosed":
                    error = ValidateResult();
                    break;
            }
            return error;
        }

        private string ValidateResult()
        {
            if (FixWarningsJoin == false &&
                FixWarningsWallsAttached == false &&
                FixWarningsRoomNotEnclosed == false)
                return "Check at least one check";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.FixWarningsJoin = FixWarningsJoin;
            Model.FixWarningsWallsAttached = FixWarningsWallsAttached;
            Model.FixWarningsRoomNotEnclosed = FixWarningsRoomNotEnclosed;

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