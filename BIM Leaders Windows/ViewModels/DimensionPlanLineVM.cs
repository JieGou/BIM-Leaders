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

        private int _selectElements;
        public int SelectElements
        {
            get { return _selectElements; }
            set
            {
                _selectElements = value;
                OnPropertyChanged(nameof(SelectElements));
            }
        }

        private string _selectElementsString;
        public string SelectElementsString
        {
            get { return _selectElementsString; }
            set
            {
                _selectElementsString = value;
                OnPropertyChanged(nameof(SelectElementsString));
            }
        }

        private string _selectElementsError;
        public string SelectElementsError
        {
            get { return _selectElementsError; }
            set
            {
                _selectElementsError = value;
                OnPropertyChanged(nameof(SelectElementsError));
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

            SelectElementsString = "No selection";

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
                case "SelectElementsString":
                    error = SelectElementsError;
                    break;
            }
            return error;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.Run();
        }

        public ICommand SelectLineCommand { get; set; }

        private void SelectLineAction()
        {
            IsVisible = false;

            SelectLineModel.Run();
            
            SelectElementsError = SelectLineModel.Error;

            if (SelectLineModel.Error.Length > 0)
            {
                SelectElements = 0;
                SelectElementsString = "No selection";
                Model.SelectElements = 0;
            }  
            else
            {
                SelectElements = SelectLineModel.SelectedElement;
                SelectElementsString = SelectElements.ToString();
                Model.SelectElements = SelectLineModel.SelectedElement;
            }
            
            IsVisible = true;
        }

        #endregion
    }
}
