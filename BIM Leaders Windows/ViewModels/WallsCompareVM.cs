using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "WallsCompare"
    /// </summary>
    public class WallsCompareVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private WallsCompareM _model;
        public WallsCompareM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private bool _checkOneLink;
        public bool CheckOneLink
        {
            get { return _checkOneLink; }
            set
            {
                _checkOneLink = value;
                OnPropertyChanged(nameof(CheckOneLink));
            }
        }

        private SortedDictionary<string, int> _materials;
        public SortedDictionary<string, int> Materials
        {
            get { return _materials; }
            set { _materials = value; }
        }

        private SortedDictionary<string, int> _fillTypes;
        public SortedDictionary<string, int> FillTypes
        {
            get { return _fillTypes; }
            set { _fillTypes = value; }
        }

        private int _materialsSelected;
        public int MaterialsSelected
        {
            get { return _materialsSelected; }
            set
            {
                _materialsSelected = value;
                OnPropertyChanged(nameof(MaterialsSelected));
            }
        }

        private int _fillTypesSelected;
        public int FillTypesSelected
        {
            get { return _fillTypesSelected; }
            set
            {
                _fillTypesSelected = value;
                OnPropertyChanged(nameof(FillTypesSelected));
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="WallsCompareVM"/> class.
        /// </summary>
        public WallsCompareVM(WallsCompareM model)
        {
            Model = model;

            CheckOneLink = true;

            RunCommand = new RunCommand(RunAction);
        }

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction()
        {
            Model.CheckOneLink = CheckOneLink;
            Model.MaterialsSelected = MaterialsSelected;
            Model.FillTypesSelected = FillTypesSelected;

            Model.Run();

            CloseAction();
        }

        public Action CloseAction { get; set; }

        #endregion
    }
}
