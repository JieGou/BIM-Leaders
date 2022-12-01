using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        #region PROPERTIES

        private BaseModel _baseModel;
        public BaseModel BaseModel
        {
            get { return _baseModel; }
            set { _baseModel = value; }
        }

        public bool Closed { get; private protected set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Set initial data for viewmodel.
        /// 1. Casting base model to specific type for viewmodel.
        /// 2. Getting data from model (list of fill types, views list, etc).
        /// 3. Setting other inputs of the view.
        /// </summary>
        public abstract void SetInitialData();

        #endregion

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

        private protected virtual string GetValidationError(string propertyName)
        {
            string error = null;
            return error;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private protected abstract void RunAction(Window window);

        public ICommand CloseCommand { get; set; }

        private protected abstract void CloseAction(Window window);

        #endregion
    }
}