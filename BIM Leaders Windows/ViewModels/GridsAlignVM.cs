using System;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "GridsAlign"
    /// </summary>
    public class GridsAlignVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private GridsAlignM _model;
        public GridsAlignM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="GridsAlignVM"/> chosen Side 1.
        /// </summary>
        /// <value><c>true</c> if Side 1 is chosen, if not, then <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating if <see cref="GridsAlignVM"/> chosen Side 2.
        /// </summary>
        /// <value><c>true</c> if Side 2 is chosen, if not, then <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="GridsAlignVM"/> class.
        /// </summary>
        public GridsAlignVM(GridsAlignM model)
        {
            Model = model;

            Side1 = true;
            Side2 = true;
            Switch2D = true;

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
            Model.Side1 = Side1;
            Model.Side2 = Side2;
            Model.Switch2D = Switch2D;
            Model.Switch3D = Switch3D;

            Model.Run();

            CloseAction();
        }

        public Action CloseAction { get; set; }

        #endregion
    }
}