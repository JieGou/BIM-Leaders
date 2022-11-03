using System;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionSectionFloors"
    /// </summary>
    public class DimensionSectionFloorsVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _minThickThicknessMinValue = 1;
        private const int _minThickThicknessMaxValue = 100;

        #region PROPERTIES

        private DimensionSectionFloorsM _model;
        public DimensionSectionFloorsM Model
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

        private bool _placeSpots;
        public bool PlaceSpots
        {
            get { return _placeSpots; }
            set
            {
                _placeSpots = value;
                OnPropertyChanged(nameof(PlaceSpots));
            }
        }

        private bool _placeOnThinTop;
        public bool PlaceOnThinTop
        {
            get { return _placeOnThinTop; }
            set
            {
                _placeOnThinTop = value;
                OnPropertyChanged(nameof(PlaceOnThinTop));
                OnPropertyChanged(nameof(PlaceOnThickTop));
                OnPropertyChanged(nameof(PlaceOnThickBot));
            }
        }
        private bool _placeOnThickTop;
        public bool PlaceOnThickTop
        {
            get { return _placeOnThickTop; }
            set
            {
                _placeOnThickTop = value;
                OnPropertyChanged(nameof(PlaceOnThinTop));
                OnPropertyChanged(nameof(PlaceOnThickTop));
                OnPropertyChanged(nameof(PlaceOnThickBot));
            }
        }
        private bool _placeOnThickBot;
        public bool PlaceOnThickBot
        {
            get { return _placeOnThickBot; }
            set
            {
                _placeOnThickBot = value;
                OnPropertyChanged(nameof(PlaceOnThinTop));
                OnPropertyChanged(nameof(PlaceOnThickTop));
                OnPropertyChanged(nameof(PlaceOnThickBot));
            }
        }

        private int _minThickThickness;
        public int MinThickThickness
        {
            get { return _minThickThickness; }
            set { _minThickThickness = value; }
        }

        private string _minThickThicknessString;
        public string MinThickThicknessString
        {
            get { return _minThickThicknessString; }
            set
            {
                _minThickThicknessString = value;
                OnPropertyChanged(nameof(_minThickThicknessString));
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
        /// Initializing a new instance of the <see cref=DimensionSectionFloorsVM"/> class.
        /// </summary>
        public DimensionSectionFloorsVM(DimensionSectionFloorsM model, SelectLineM selectLineModel)
        {
            Model = model;
            SelectLineModel = selectLineModel;

            IsVisible = true;

            PlaceSpots = true;
            PlaceOnThinTop = true;
            PlaceOnThickTop = true;
            PlaceOnThickBot = true;
            MinThickThickness = 15;
            MinThickThicknessString = MinThickThickness.ToString();

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
                case "PlaceOnThinTop":
                    error = ValidatePlacement();
                    break;
                case "PlaceOnThickTop":
                    error = ValidatePlacement();
                    break;
                case "PlaceOnThickBot":
                    error = ValidatePlacement();
                    break;
                case "MinThickThicknessString":
                    error = ValidateIsWholeNumber(out int thickness, MinThickThicknessString);
                    if (string.IsNullOrEmpty(error))
                    {
                        MinThickThickness = thickness;
                        error = ValidateMinThickThickness();
                    }
                    break;
                case "SelectedElementString":
                    if (SelectLineModel.Error?.Length > 0)
                        error = SelectLineModel.Error;
                    if (SelectedElement == 0)
                        error = "No selection";
                    break;
            }
            return error;
        }

        private string ValidatePlacement()
        {
            if (PlaceOnThinTop == false && PlaceOnThickTop == false
                && PlaceOnThickBot == false)
                return "Check at least one placement";
            return null;
        }

        private string ValidateIsWholeNumber(out int numberParsed, string number)
        {
            numberParsed = 0;

            if (string.IsNullOrEmpty(number))
                return "Input is empty";
            if (!int.TryParse(number, out numberParsed))
                return "Not a whole number";

            return null;
        }

        private string ValidateMinThickThickness()
        {
            if (MinThickThickness < _minThickThicknessMinValue || MinThickThickness > _minThickThicknessMaxValue)
                return $"From {_minThickThicknessMinValue} to {_minThickThicknessMaxValue} cm";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.PlaceSpots = PlaceSpots;

            Model.PlacementThinTop = PlaceOnThinTop;
            Model.PlacementThickTop = PlaceOnThickTop;
            Model.PlacementThickBot = PlaceOnThickBot;
            Model.MinThickThicknessCm = MinThickThickness;
            Model.SelectedElement = SelectedElement;

            Model.Run();

            CloseAction();
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

        public Action CloseAction { get; set; }

        #endregion
    }
}