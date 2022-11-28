using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WarningsSolve"
    /// </summary>
    public class WarningsSolveViewModel : BaseViewModel
    {
        #region PROPERTIES

        private WarningsSolveModel _model;
        public WarningsSolveModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

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

        public WarningsSolveViewModel()
        {
            FixWarningsJoin = true;
            FixWarningsWallsAttached = false;
            FixWarningsRoomNotEnclosed = true;

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

        private protected override void RunAction(Window window)
        {
            Model = BaseModel as WarningsSolveModel;

            Model.FixWarningsJoin = FixWarningsJoin;
            Model.FixWarningsWallsAttached = FixWarningsWallsAttached;
            Model.FixWarningsRoomNotEnclosed = FixWarningsRoomNotEnclosed;

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