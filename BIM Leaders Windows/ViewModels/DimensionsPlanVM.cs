using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionsPlan"
    /// </summary>
    public class DimensionsPlanVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int _searchStepMinValue = 1;
        private const int _searchStepMaxValue = 100;
        private const int _searchDistanceMinValue = 100;
        private const int _searchDistanceMaxValue = 10000;
        private const int _minReferencesMinValue = 0;
        private const int _minReferencesMaxValue = 10;

        #region PROPERTIES

        private DimensionsPlanM _model;
        public DimensionsPlanM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public bool Closed { get; private set; }

        private string _searchStepString;
        public string SearchStepString
        {
            get { return _searchStepString; }
            set
            {
                _searchStepString = value;
                OnPropertyChanged(nameof(SearchStepString));
            }
        }
        private double _searchStep;
        public double SearchStep
        {
            get { return _searchStep; }
            set { _searchStep = value; }
        }

        private string _searchDistanceString;
        public string SearchDistanceString
        {
            get { return _searchDistanceString; }
            set
            {
                _searchDistanceString = value;
                OnPropertyChanged(nameof(SearchDistanceString));
            }
        }
        private double _searchDistance;
        public double SearchDistance
        {
            get { return _searchDistance; }
            set { _searchDistance = value; }
        }

        private string _minReferencesString;
        public string MinReferencesString
        {
            get { return _minReferencesString; }
            set
            {
                _minReferencesString = value;
                OnPropertyChanged(nameof(MinReferencesString));
            }
        }
        private double _minReferences;
        public double MinReferences
        {
            get { return _minReferences; }
            set { _minReferences = value; }
        }

        #endregion

        public DimensionsPlanVM(DimensionsPlanM model)
        {
            Model = model;

            SearchStep = 15;
            SearchStepString = SearchStep.ToString();
            SearchDistance = 1500;
            SearchDistanceString = SearchDistance.ToString();
            MinReferences = 5;
            MinReferencesString = MinReferences.ToString();

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
                case "SearchStepString":
                    error = ValidateInputIsWholeNumber(out int searchStep, _searchStepString);
                    if (string.IsNullOrEmpty(error))
                    {
                        SearchStep = searchStep;
                        error = ValidateResultSearchStep();
                    }
                    break;
                case "SearchDistanceString":
                    error = ValidateInputIsWholeNumber(out int searchDistance, _searchDistanceString);
                    if (string.IsNullOrEmpty(error))
                    {
                        SearchDistance = searchDistance;
                        error = ValidateResultSearchDistance();
                    }
                    break;
                case "MinReferencesString":
                    error = ValidateInputIsWholeNumber(out int minreferences, _minReferencesString);
                    if (string.IsNullOrEmpty(error))
                    {
                        MinReferences = minreferences;
                        error = ValidateResultMinReferences();
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

        private string ValidateResultSearchStep()
        {
            if (SearchStep < _searchStepMinValue || SearchStep > _searchStepMaxValue)
                return $"From {_searchStepMinValue} to {_searchStepMaxValue} cm";
            return null;
        }

        private string ValidateResultSearchDistance()
        {
            if (SearchDistance < _searchDistanceMinValue || SearchDistance > _searchDistanceMaxValue)
                return $"From {_searchDistanceMinValue} to {_searchDistanceMaxValue} cm";
            return null;
        }

        private string ValidateResultMinReferences()
        {
            if (MinReferences < _minReferencesMinValue || MinReferences > _minReferencesMaxValue)
                return $"From {_minReferencesMinValue} to {_minReferencesMaxValue}";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.SearchDistanceCm = SearchDistance;
            Model.SearchStepCm = SearchStep;
            Model.MinReferences = MinReferences;

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