using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WallsParallel"
    /// </summary>
    public class WallsParallelVM : INotifyPropertyChanged, IDataErrorInfo
    {
        #region PROPERTIES

        private WallsParallelM _model;
        public WallsParallelM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SelectReferencePlaneM _selectReferencePlaneModel;
        public SelectReferencePlaneM SelectReferencePlaneModel
        {
            get { return _selectReferencePlaneModel; }
            set { _selectReferencePlaneModel = value; }
        }

        public bool Closed { get; private set; }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private Color _filterColor;
        public Color FilterColor
        {
            get { return _filterColor; }
            set
            {
                _filterColor = value;
                OnPropertyChanged(nameof(FilterColor));
            }
        }

        private int _selectedElement;
        public int SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                _selectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        private string _selectedElementString;
        public string SelectedElementString
        {
            get { return _selectedElementString; }
            set
            {
                _selectedElementString = value;
                OnPropertyChanged(nameof(SelectedElementString));
            }
        }

        private string _selectedElementError;
        public string SelectedElementError
        {
            get { return _selectedElementError; }
            set
            {
                _selectedElementError = value;
                OnPropertyChanged(nameof(SelectedElementError));
            }
        }

        #endregion

        public WallsParallelVM(WallsParallelM model, SelectReferencePlaneM selectModel)
        {
            Model = model;
            SelectReferencePlaneModel = selectModel;

            IsVisible = true;

            FilterColor = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };

            SelectedElement = 0;
            SelectedElementString = "No selection";

            RunCommand = new CommandWindow(RunAction);
            SelectReferencePlaneCommand = new CommandGeneric(SelectReferencePlaneAction);
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
                case "SelectedElementString":
                    if (SelectReferencePlaneModel.Error?.Length > 0)
                        error = SelectReferencePlaneModel.Error;
                    if (SelectedElement == 0)
                        error = "No selection";
                    break;
            }
            return error;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.FilterColorSystem = FilterColor;
            Model.SelectedElement = SelectedElement;

            Model.Run();

            CloseAction(window);
        }

        public ICommand SelectReferencePlaneCommand { get; set; }

        private void SelectReferencePlaneAction()
        {
            IsVisible = false;

            SelectReferencePlaneModel.Run();

            SelectedElement = SelectReferencePlaneModel.SelectedElement;

            SelectedElementString = (SelectedElement == 0)
                ? "No selection"
                : $"{SelectedElement}";
            SelectedElementError = SelectReferencePlaneModel.Error;

            IsVisible = true;
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