using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Levels_Align"
    /// </summary>
    public class LevelsAlignData : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="LevelsAlignData"/> class.
        /// </summary>
        public LevelsAlignData()
        {
            ResultSide1 = true;
            ResultSide2 = true;
            ResultSwitch2D = true;
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="GridsAlignData"/> chosen Side 1.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Side 1 is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _resultSide1;
        public bool ResultSide1
        {
            get { return _resultSide1; }
            set
            {
                _resultSide1 = value;
                OnPropertyChanged(nameof(ResultSide1));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="GridsAlignData"/> chosen Side 2.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Side 2 is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _resultSide2;
        public bool ResultSide2
        {
            get { return _resultSide2; }
            set
            {
                _resultSide2 = value;
                OnPropertyChanged(nameof(ResultSide2));
            }
        }

        private bool _resultSwitch2D;
        public bool ResultSwitch2D
        {
            get { return _resultSwitch2D; }
            set
            {
                _resultSwitch2D = value;
                OnPropertyChanged(nameof(ResultSwitch2D));
            }
        }
        private bool _resultSwitch3D;
        public bool ResultSwitch3D
        {
            get { return _resultSwitch3D; }
            set
            {
                _resultSwitch3D = value;
                OnPropertyChanged(nameof(ResultSwitch3D));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
