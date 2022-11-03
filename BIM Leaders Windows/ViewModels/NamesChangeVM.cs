﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "NamesChange"
    /// </summary>
    public class NamesChangeVM : INotifyPropertyChanged, IDataErrorInfo
    {

        #region PROPERTIES

        private NamesChangeM _model;
        public NamesChangeM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private string _substringOld;
        public string SubstringOld
        {
            get { return _substringOld; }
            set
            {
                _substringOld = value;
                OnPropertyChanged(nameof(SubstringOld));
            }
        }

        private string _substringNew;
        public string SubstringNew
        {
            get { return _substringNew; }
            set
            {
                _substringNew = value;
                OnPropertyChanged(nameof(SubstringNew));
            }
        }

        private bool _partPrefix { get; set; }
        public bool PartPrefix
        {
            get { return _partPrefix; }
            set
            {
                _partPrefix = value;
                OnPropertyChanged(nameof(PartPrefix));
            }
        }
        private bool _partCenter { get; set; }
        public bool PartCenter
        {
            get { return _partCenter; }
            set
            {
                _partCenter = value;
                OnPropertyChanged(nameof(PartCenter));
            }
        }
        private bool _partSuffix { get; set; }
        public bool PartSuffix
        {
            get { return _partSuffix; }
            set
            {
                _partSuffix = value;
                OnPropertyChanged(nameof(PartSuffix));
            }
        }

        private List<bool> _selectedCategories { get; set; }
        public List<bool> SelectedCategories
        {
            get { return _selectedCategories; }
            set
            {
                _selectedCategories = value;
                OnPropertyChanged(nameof(SelectedCategories));
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="NamesChangeVM"/> class.
        /// </summary>
        public NamesChangeVM(NamesChangeM model)
        {
            Model = model;

            SubstringOld = "OLD";
            SubstringNew = "NEW";
            PartPrefix = true;
            SelectedCategories = Enumerable.Repeat(false, 24).ToList();
            SelectedCategories[6] = true;

            RunCommand = new RunCommand(RunAction);
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
                case "SubstringOld":
                    error = ValidateSubstringOld();
                    break;
                case "SubstringNew":
                    error = ValidateSubstringNew();
                    break;
                case "SelectedCategories":
                    error = ValidateSelectedCategories();
                    break;
            }
            return error;
        }

        private string ValidateSubstringOld()
        {
            if (string.IsNullOrEmpty(SubstringOld))
                return "Input is empty";
            else
            {
                if (SubstringOld.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateSubstringNew()
        {
            if (string.IsNullOrEmpty(SubstringNew))
                return "Input is empty";
            else
            {
                if (SubstringNew.Length < 2)
                    return "From 2 symbols";
            }
            return null;
        }

        private string ValidateSelectedCategories()
        {
            if (!SelectedCategories.Contains(true))
                return "Categories not checked";
            return null;
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.PartPrefix = PartPrefix;
            Model.PartSuffix = PartSuffix;
            Model.SelectedCategories = SelectedCategories;
            Model.SubstringOld = SubstringOld;
            Model.SubstringNew = SubstringNew;

            Model.Run();

            CloseAction();
        }

        public Action CloseAction { get; set; }

        #endregion
    }
}