using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Grids_Align"
    /// </summary>
    public class GridsAlignData : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="GridsAlignData"/> class.
        /// </summary>
        public GridsAlignData()
        {
            ResultSide1 = true;
            ResultSide2 = true;
            ResultSwitch = true;
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

        /// <summary>
        /// Gets or sets a value indicating if <see cref="GridsAlignData"/> chosen Switch to 2D.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Switch to 2D is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _resultSwitch;
        public bool ResultSwitch
        {
            get { return _resultSwitch; }
            set
            {
                _resultSwitch = value;
                OnPropertyChanged(nameof(ResultSwitch));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
