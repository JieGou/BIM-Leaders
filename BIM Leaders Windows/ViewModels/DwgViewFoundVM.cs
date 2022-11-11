using System.Data;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DwgViewFound"
    /// </summary>
    public class DwgViewFoundVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private DwgViewFoundM _model;
        public DwgViewFoundM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private DataSet _dwgList;
        public DataSet DwgList
        {
            get { return _dwgList; }
            set { _dwgList = value; }
        }

        private DataRowView _selectedDwg;
        public DataRowView SelectedDwg
        {
            get { return _selectedDwg; }
            set
            {
                _selectedDwg = value;
                OnPropertyChanged(nameof(SelectedDwg));
            }
        }

        #endregion

        public DwgViewFoundVM(DwgViewFoundM model)
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
            Model.SelectedDwg = SelectedDwg.Row[2].ToString();

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