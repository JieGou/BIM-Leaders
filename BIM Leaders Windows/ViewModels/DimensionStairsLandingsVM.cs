using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionStairsLandings"
    /// </summary>
    public class DimensionStairsLandingsVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private const double _distanceMinValue = 100;
        private const double _distanceMaxValue = 200;

        #region PROPERTIES

        private DimensionStairsLandingsM _model;
        public DimensionStairsLandingsM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private bool _placeDimensionsTop;
        public bool PlaceDimensionsTop
        {
            get { return _placeDimensionsTop; }
            set
            {
                _placeDimensionsTop = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private bool _placeDimensionsMid;
        public bool PlaceDimensionsMid
        {
            get { return _placeDimensionsMid; }
            set
            {
                _placeDimensionsMid = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private bool _placeDimensionsBot;
        public bool PlaceDimensionsBot
        {
            get { return _placeDimensionsBot; }
            set
            {
                _placeDimensionsBot = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private bool _placeElevationsTop;
        public bool PlaceElevationsTop
        {
            get { return _placeElevationsTop; }
            set
            {
                _placeElevationsTop = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private bool _placeElevationsMid;
        public bool PlaceElevationsMid
        {
            get { return _placeElevationsMid; }
            set
            {
                _placeElevationsMid = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private bool _placeElevationsBot;
        public bool PlaceElevationsBot
        {
            get { return _placeElevationsBot; }
            set
            {
                _placeElevationsBot = value;
                OnPropertyChanged(nameof(PlaceDimensionsTop));
                OnPropertyChanged(nameof(PlaceDimensionsMid));
                OnPropertyChanged(nameof(PlaceDimensionsBot));
                OnPropertyChanged(nameof(PlaceElevationsTop));
                OnPropertyChanged(nameof(PlaceElevationsMid));
                OnPropertyChanged(nameof(PlaceElevationsBot));
            }
        }

        private string _distanceString;
        public string DistanceString
        {
            get { return _distanceString; }
            set
            {
                _distanceString = value;
                OnPropertyChanged(nameof(DistanceString));
            }
        }
        private double _distance;
        public double Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        #endregion

        public DimensionStairsLandingsVM(DimensionStairsLandingsM model)
        {
            Model = model;

            PlaceDimensionsTop = true;
            PlaceDimensionsMid = true;
            PlaceDimensionsBot = true;
            PlaceElevationsTop = true;
            PlaceElevationsMid = true;
            PlaceElevationsBot = true;
            Distance = 150;
            DistanceString = Distance.ToString();

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
                case "PlaceDimensionsTop":
                    error = ValidateResultPlacement();
                    break;
                case "PlaceDimensionsMid":
                    error = ValidateResultPlacement();
                    break;
                case "PlaceDimensionsBot":
                    error = ValidateResultPlacement();
                    break;
                case "PlaceElevationsTop":
                    error = ValidateResultPlacement();
                    break;
                case "PlaceElevationsMid":
                    error = ValidateResultPlacement();
                    break;
                case "PlaceElevationsBot":
                    error = ValidateResultPlacement();
                    break;
                case "DistanceString":
                    error = ValidateInputIsWholeNumber(out int distance, DistanceString);
                    if (string.IsNullOrEmpty(error))
                    {
                        Distance = distance;
                        error = ValidateResultDistance();
                    }
                    break;
            }
            return error;
        }

        private string ValidateResultPlacement()
        {
            if (PlaceDimensionsTop == false && PlaceDimensionsMid == false &&
                PlaceDimensionsBot == false && PlaceElevationsTop == false &&
                PlaceElevationsMid == false && PlaceElevationsBot == false)
                return "Check at least one placement";
            return null;
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

        private string ValidateResultDistance()
        {
            if (Distance < _distanceMinValue || Distance > _distanceMaxValue)
                return $"From {_distanceMinValue} to {_distanceMaxValue} cm";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.PlaceDimensionsTop = PlaceDimensionsTop;
            Model.PlaceDimensionsMid = PlaceDimensionsMid;
            Model.PlaceDimensionsBot = PlaceDimensionsBot;
            Model.PlaceElevationsTop = PlaceElevationsTop;
            Model.PlaceElevationsMid = PlaceElevationsMid;
            Model.PlaceElevationsBot = PlaceElevationsBot;
            Model.DistanceCm = Distance;

            Model.Run();

            CloseAction(window);
        }

        public ICommand CloseCommand { get; set; }

        private void CloseAction(Window window)
        {
            if (window != null)
                window.Close();
        }

        #endregion
    }
}