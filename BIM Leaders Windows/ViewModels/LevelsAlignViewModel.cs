using System.Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "LevelsAlign"
    /// </summary>
    public class LevelsAlignViewModel : BaseViewModel
    {
        #region PROPERTIES

        private LevelsAlignModel _model;
        public LevelsAlignModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private bool _side1;
        public bool Side1
        {
            get { return _side1; }
            set
            {
                _side1 = value;
                OnPropertyChanged(nameof(Side1));
            }
        }

        private bool _side2;
        public bool Side2
        {
            get { return _side2; }
            set
            {
                _side2 = value;
                OnPropertyChanged(nameof(Side2));
            }
        }

        private bool _switch2D;
        public bool Switch2D
        {
            get { return _switch2D; }
            set
            {
                _switch2D = value;
                OnPropertyChanged(nameof(Switch2D));
            }
        }
        private bool _switch3D;
        public bool Switch3D
        {
            get { return _switch3D; }
            set
            {
                _switch3D = value;
                OnPropertyChanged(nameof(Switch3D));
            }
        }

        #endregion

        public LevelsAlignViewModel()
        {
            RunCommand = new CommandWindow(RunAction);
            CloseCommand = new CommandWindow(CloseAction);
        }

        #region METHODS

        public override void SetInitialData()
        {
            Model = (LevelsAlignModel)BaseModel;

            Side1 = true;
            Side2 = true;
            Switch2D = true;
        }

        #endregion

        #region COMMANDS

        private protected override void RunAction(Window window)
        {
            Model.Side1 = Side1;
            Model.Side2 = Side2;
            Model.Switch2D = Switch2D;
            Model.Switch3D = Switch3D;

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