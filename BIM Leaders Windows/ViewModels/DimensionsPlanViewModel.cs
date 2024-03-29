﻿using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionsPlan"
    /// </summary>
    public class DimensionsPlanViewModel : BaseViewModel
    {
        private const int _searchStepMinValue = 1;
        private const int _searchStepMaxValue = 100;
        private const int _searchDistanceMinValue = 100;
        private const int _searchDistanceMaxValue = 10000;
        private const int _minReferencesMinValue = 0;
        private const int _minReferencesMaxValue = 10;
        private const int _maxUnionDistanceMinValue = 0;
        private const int _maxUnionDistanceMaxValue = 1000;

        #region PROPERTIES

        private DimensionsPlanModel _model;
        public DimensionsPlanModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

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

        private string _maxUnionDistanceString;
        public string MaxUnionDistanceString
        {
            get { return _maxUnionDistanceString; }
            set
            {
                _maxUnionDistanceString = value;
                OnPropertyChanged(nameof(MaxUnionDistanceString));
            }
        }
        private double _maxUnionDistance;
        public double MaxUnionDistance
        {
            get { return _maxUnionDistance; }
            set { _maxUnionDistance = value; }
        }

        #endregion

        public DimensionsPlanViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (DimensionsPlanModel)BaseModel;

            SearchStep = 15;
            SearchStepString = SearchStep.ToString();
            SearchDistance = 1500;
            SearchDistanceString = SearchDistance.ToString();
            MinReferences = 5;
            MinReferencesString = MinReferences.ToString();
            MaxUnionDistance = 30;
            MaxUnionDistanceString= MaxUnionDistance.ToString();
        }

        #endregion

        #region VALIDATION

        private protected override string GetValidationError(string propertyName)
        {
            string error = null;

            switch (propertyName)
            {
                case "SearchStepString":
                    error = ValidateInputIsWholeNumber(out int searchStep, SearchStepString);
                    if (string.IsNullOrEmpty(error))
                    {
                        SearchStep = searchStep;
                        error = ValidateResultSearchStep();
                    }
                    break;
                case "SearchDistanceString":
                    error = ValidateInputIsWholeNumber(out int searchDistance, SearchDistanceString);
                    if (string.IsNullOrEmpty(error))
                    {
                        SearchDistance = searchDistance;
                        error = ValidateResultSearchDistance();
                    }
                    break;
                case "MinReferencesString":
                    error = ValidateInputIsWholeNumber(out int minReferences, MinReferencesString);
                    if (string.IsNullOrEmpty(error))
                    {
                        MinReferences = minReferences;
                        error = ValidateResultMinReferences();
                    }
                    break;
                case "MaxUnionDistanceString":
                    error = ValidateInputIsWholeNumber(out int maxUnionDistance, MaxUnionDistanceString);
                    if (string.IsNullOrEmpty(error))
                    {
                        MaxUnionDistance = maxUnionDistance;
                        error = ValidateResultMaxUnionDistance();
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

        private string ValidateResultMaxUnionDistance()
        {
            if (MaxUnionDistance < _maxUnionDistanceMinValue || MaxUnionDistance > _maxUnionDistanceMaxValue)
                return $"From {_maxUnionDistanceMinValue} to {_maxUnionDistanceMaxValue}";
            return null;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.SearchDistanceCm = SearchDistance;
            Model.SearchStepCm = SearchStep;
            Model.MinReferences = MinReferences;
            Model.MaxUnionDistanceCm = MaxUnionDistance;

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