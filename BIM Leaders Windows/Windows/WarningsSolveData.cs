using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "WarningsSolve"
    /// </summary>
    public class WarningsSolveData : INotifyPropertyChanged, IDataErrorInfo
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WarningsSolveData"/> class.
        /// </summary>
        public WarningsSolveData()
        {
            ResultFixWarningsJoin = true;
            ResultFixWarningsWallsAttached = false;
            ResultFixWarningsRoomNotEnclosed = true;
        }

        private bool _resultFixWarningsJoin;
        public bool ResultFixWarningsJoin
        {
            get { return _resultFixWarningsJoin; }
            set
            {
                _resultFixWarningsJoin = value;
                OnPropertyChanged(nameof(ResultFixWarningsJoin));
                OnPropertyChanged(nameof(ResultFixWarningsWallsAttached));
                OnPropertyChanged(nameof(ResultFixWarningsRoomNotEnclosed));
            }
        }

        private bool _resultFixWarningsWallsAttached;
        public bool ResultFixWarningsWallsAttached
        {
            get { return _resultFixWarningsWallsAttached; }
            set
            {
                _resultFixWarningsWallsAttached = value;
                OnPropertyChanged(nameof(ResultFixWarningsJoin));
                OnPropertyChanged(nameof(ResultFixWarningsWallsAttached));
                OnPropertyChanged(nameof(ResultFixWarningsRoomNotEnclosed));
            }
        }

        private bool _resultFixWarningsRoomNotEnclosed;
        public bool ResultFixWarningsRoomNotEnclosed
        {
            get { return _resultFixWarningsRoomNotEnclosed; }
            set
            {
                _resultFixWarningsRoomNotEnclosed = value;
                OnPropertyChanged(nameof(ResultFixWarningsJoin));
                OnPropertyChanged(nameof(ResultFixWarningsWallsAttached));
                OnPropertyChanged(nameof(ResultFixWarningsRoomNotEnclosed));
            }
        }

        #region Validation

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
                case "ResultFixWarningsJoin":
                    error = ValidateResult();
                    break;
                case "ResultFixWarningsWallsAttached":
                    error = ValidateResult();
                    break;
                case "ResultFixWarningsRoomNotEnclosed":
                    error = ValidateResult();
                    break;
            }
            return error;
        }

        private string ValidateResult()
        {
            if (ResultFixWarningsJoin == false && ResultFixWarningsWallsAttached == false
                && ResultFixWarningsRoomNotEnclosed == false)
                return "Check at least one check";
            return null;
        }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
