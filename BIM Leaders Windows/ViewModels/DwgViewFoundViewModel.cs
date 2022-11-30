using System.Data;
using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DwgViewFound"
    /// </summary>
    public class DwgViewFoundViewModel : BaseViewModel
    {
        #region PROPERTIES

        private DwgViewFoundModel _model;
        public DwgViewFoundModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private DataSet _dwgList;
        public DataSet DwgList
        {
            get { return _dwgList; }
            set
            {
                _dwgList = value;
                OnPropertyChanged(nameof(DwgList));
            }
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

        public DwgViewFoundViewModel()
        {
            //DwgList = Model.DwgTable;

            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (DwgViewFoundModel)BaseModel;

            DwgList = Model.GetDwgTable();
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            //Model = (DwgViewFoundModel)BaseModel;

            Model.SelectedDwg = SelectedDwg.Row[2].ToString();

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