using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Input;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// View model for command "DimensionsPlanCheck"
    /// </summary>
    public class DimensionsPlanCheckVM : INotifyPropertyChanged
    {
        #region PROPERTIES

        private DimensionsPlanCheckM _model;
        public DimensionsPlanCheckM Model
        {
            get { return _model; }
            set { _model = value; }
        }

        private Color _filterColor;
        public Color FilterColor
        {
            get { return _filterColor; }
            set
            {
                _filterColor = value;
                OnPropertyChanged(nameof(FilterColor));
            }
        }

        #endregion

        public DimensionsPlanCheckVM(DimensionsPlanCheckM model)
        {
            Model = model;

            FilterColor = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };

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
            Model.FilterColorSystem = FilterColor;

            Model.Run();
        }

        #endregion
    }
}
