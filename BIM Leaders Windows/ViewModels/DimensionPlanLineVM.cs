using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionPlanLine"
    /// </summary>
    public class DimensionPlanLineVM : INotifyPropertyChanged, IDataErrorInfo
    {
        #region PROPERTIES

        private DimensionPlanLineM _model;
        public DimensionPlanLineM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SelectLineM _selectLineModel;
        public SelectLineM SelectLineModel
        {
            get { return _selectLineModel; }
            set { _selectLineModel = value; }
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

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref=DimensionPlanLineVM"/> class.
        /// </summary>
        public DimensionPlanLineVM(DimensionPlanLineM model, SelectLineM selectLineModel)
        {
            Model = model;
            SelectLineModel = selectLineModel;

            IsVisible = true;

            SelectedElement = 0;
            SelectedElementString = "No selection";

            RunCommand = new RunCommand(RunAction);
            SelectLineCommand = new RunCommand(SelectLineAction);
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
                    if (SelectLineModel.Error?.Length > 0)
                        error = SelectLineModel.Error;
                    if (SelectedElement == 0)
                        error = "No selection";
                    break;
            }
            return error;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.SelectedElement = SelectedElement;

            Model.Run();
        }

        public ICommand SelectLineCommand { get; set; }

        private void SelectLineAction()
        {
            IsVisible = false;

            SelectLineModel.Run();
            
            SelectedElement = SelectLineModel.SelectedElement;
            SelectedElementString = (SelectedElement == 0)
                ? "No selection"
                : SelectedElement.ToString();
            SelectedElementError = SelectLineModel.Error;
            
            IsVisible = true;
        }

        #endregion
    }
}
