using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WallsParallel"
    /// </summary>
    public class WallsParallelViewModel : BaseViewModel
    {
        #region PROPERTIES

        private WallsParallelModel _model;
        public WallsParallelModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SelectReferencePlaneModel _selectReferencePlaneModel;
        public SelectReferencePlaneModel SelectReferencePlaneModel
        {
            get { return _selectReferencePlaneModel; }
            set { _selectReferencePlaneModel = value; }
        }

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

        public WallsParallelViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            SelectReferencePlaneCommand = new CommandGeneric(SelectReferencePlaneAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (WallsParallelModel)BaseModel;

            IsVisible = true;

            FilterColor = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };

            SelectedElement = 0;
            SelectedElementString = "No selection";
        }

        #endregion

        #region VALIDATION

        private protected override string GetValidationError(string propertyName)
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

        private protected override void RunAction(Window window)
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

            SelectReferencePlaneModel = new SelectReferencePlaneModel(BaseModel);
            SelectReferencePlaneModel.Run();

            SelectedElement = SelectReferencePlaneModel.SelectedElement;

            SelectedElementString = (SelectedElement == 0)
                ? "No selection"
                : $"{SelectedElement}";
            SelectedElementError = SelectReferencePlaneModel.Error;

            IsVisible = true;
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