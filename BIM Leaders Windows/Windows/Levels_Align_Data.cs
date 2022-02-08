using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Levels_Align"
    /// </summary>
    public class Levels_Align_Data : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Levels_Align_Data"/> class.
        /// </summary>
        public Levels_Align_Data()
        {
            result_side_1 = true;
            result_side_2 = true;
            result_switch = true;
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Grids_Align_Data"/> chosen Side 1.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Side 1 is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _result_side_1;
        public bool result_side_1
        {
            get { return _result_side_1; }
            set
            {
                _result_side_1 = value;
                OnPropertyChanged(nameof(result_side_1));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Grids_Align_Data"/> chosen Side 2.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Side 2 is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _result_side_2;
        public bool result_side_2
        {
            get { return _result_side_2; }
            set
            {
                _result_side_2 = value;
                OnPropertyChanged(nameof(result_side_2));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Levels_Align_Data"/> chosen Switch to 2D.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Switch to 2D is chosen, if not, then <c>false</c>.
        /// </value>
        private bool _result_switch;
        public bool result_switch
        {
            get { return _result_switch; }
            set
            {
                _result_switch = value;
                OnPropertyChanged(nameof(result_switch));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Levels_Align_Data GetInformation()
        {
            // Information gathered from window
            var information = new Levels_Align_Data();
            information.result_side_1 = result_side_1;
            information.result_side_2 = result_side_2;
            information.result_switch = result_switch;
            return information;
        }
    }
}
