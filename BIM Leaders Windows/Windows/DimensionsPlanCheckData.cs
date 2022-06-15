using System.Windows.Media;
using System.ComponentModel;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command "Dimension_Section_Floors"
    /// </summary>
    public class DimensionsPlanCheckData : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DimensionsPlanCheckData"/> class.
        /// </summary>
        public DimensionsPlanCheckData()
        {
            ResultColor = new Color
            {
                R = 255,
                G = 127,
                B = 39
            };
        }

        private Color _resultColor;
        public Color ResultColor
        {
            get { return _resultColor; }
            set
            {
                _resultColor = value;
                OnPropertyChanged(nameof(ResultColor));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
