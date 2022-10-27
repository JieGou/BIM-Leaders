using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Renamer form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class NamesChangeForm : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="NamesChangeForm"/>
        /// </summary>
        public NamesChangeForm()
        {
            InitializeMaterialDesign();
            InitializeComponent();
        }

        private void InitializeMaterialDesign()
        {
            // Create dummy objects to force the MaterialDesign assemblies to be loaded
            // from this assembly, which causes the MaterialDesign assemblies to be searched
            // relative to this assembly's path. Otherwise, the MaterialDesign assemblies
            // are searched relative to Eclipse's path, so they're not found.
            Card card = new Card();
            Hue hue = new Hue("Dummy", Colors.Black, Colors.White);
        }

        // Select all categories if "Select all" label was clicked.
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<CheckBox> checkBoxList = new ObservableCollection<CheckBox>(){
                checkBox00, checkBox01, checkBox02, checkBox03, checkBox04, checkBox05, checkBox06, checkBox07,
                checkBox08, checkBox09, checkBox10, checkBox11, checkBox12, checkBox13, checkBox14, checkBox15,
                checkBox16, checkBox17, checkBox18, checkBox19, checkBox20, checkBox21, checkBox22, checkBox23,
            };

            foreach (CheckBox c in checkBoxList)
            {
                c.IsChecked = true;
            }
        }

        private void ButtonExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Button_ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonOkClick(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Move the window
        private void FormMouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
