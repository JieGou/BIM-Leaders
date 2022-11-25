using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
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

        public bool Closed { get; private set; }

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

        public DwgNameDeleteVM(DwgNameDeleteM model)
        {
            Model = model;

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

        #region COMMANDS

        public ICommand RunCommand { get; set; }

        private void RunAction(Window window)
        {
            Model.DwgListSelected = DwgListSelected;

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