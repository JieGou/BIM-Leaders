using System.Collections.Generic;
using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DwgNameDelete"
    /// </summary>
    public class DwgNameDeleteViewModel : BaseViewModel
    {
        #region PROPERTIES

        private DwgNameDeleteModel _model;
        public DwgNameDeleteModel Model
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

        public DwgNameDeleteViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model = BaseModel as DwgNameDeleteModel;

            Model.DwgListSelected = DwgListSelected;

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