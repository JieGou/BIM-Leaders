using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class Dimension_Section_Floors_Form_R : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Dimension_Section_Floors_Form_R"/>
        /// </summary>
        public Dimension_Section_Floors_Form_R()
        {
            InitializeComponent();
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

        // Move the window
        private void Dimension_Section_Floors_Form_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void input_thickness_TextChanged(object sender, System.EventArgs e)
        {
            string result_thickness = in_thickness.Text;
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Dimension_Section_Floors_Data GetInformation()
        {
            // Information gathered from window
            var information = new Dimension_Section_Floors_Data()
            {
                result_spots = (bool)rb_1.IsChecked,
                result_thickness = double.Parse(in_thickness.Text)
            };
            return information;
        }
    }
}
