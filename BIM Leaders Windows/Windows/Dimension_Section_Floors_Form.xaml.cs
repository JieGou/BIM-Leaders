using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class Dimension_Section_Floors_Form : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Dimension_Section_Floors_Form"/>
        /// </summary>
        public Dimension_Section_Floors_Form()
        {
            InitializeComponent();

            DataContext = new Dimension_Section_Floors_Data();
        }

        /// <summary>
        /// Handles the Click event of the Button_exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_exit_Click(object sender, System.EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Button_ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_ok_Click(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Move the window
        private void Dimension_Section_Floors_Form_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
