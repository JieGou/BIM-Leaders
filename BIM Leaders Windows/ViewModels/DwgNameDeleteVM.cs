using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DwgNameDelete"
    /// </summary>
    public class DwgNameDeleteVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private DwgNameDeleteM _model;
        public DwgNameDeleteM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private SortedDictionary<string, int> _dwgList = new SortedDictionary<string, int>();
        public SortedDictionary<string, int> DwgList
        {
            get { return _dwgList; }
            set { _dwgList = value; }
        }

        private int _dwgListSelected;
        public int DwgListSelected
        {
            get { return _dwgListSelected; }
            set
            {
                _dwgListSelected = value;
                OnPropertyChanged(nameof(DwgListSelected));
            }
        }

        #endregion

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DwgNameDeleteVM"/> class.
        /// </summary>
        public DwgNameDeleteVM(DwgNameDeleteM model)
        {
            Model = model;

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
            Model.DwgListSelected = DwgListSelected;

            Model.Run();

            CloseAction();
        }

        public Action CloseAction { get; set; }

        #endregion

    }
}
