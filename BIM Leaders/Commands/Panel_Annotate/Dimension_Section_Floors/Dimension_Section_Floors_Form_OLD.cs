using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Dimension_Section_Floors_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Dimension_Section_Floors_Form"/>
        /// </summary>
        public Dimension_Section_Floors_Form()
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
            DialogResult = DialogResult.OK;
            Close();
        }
        /// <summary>
        /// Handles the Click event of the Button_exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_exit_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // Tooltip
        private void label1_MouseHover(object sender, System.EventArgs e)
        {
            toolTip_thickness.Show("From 0 to 50 cm", label1);
        }

        System.Drawing.Point last_point;
        private void Dimension_Section_Floors_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Dimension_Section_Floors_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
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
                result_spots = in_rb1.Checked,
                result_thickness = in_thickness.Value
            };
            return information;
        }

        bool result_spots_1 = false;
        bool result_spots_2 = false;
        decimal result_thickness = 10;

        private void In_thickness_ValueChanged(object sender, System.EventArgs e)
        {
            result_thickness = in_thickness.Value;
        }
        public decimal Result_Thickness()
        {
            return result_thickness;
        }

        private void In_rb1_CheckedChanged(object sender, System.EventArgs e)
        {
            result_spots_1 = true;
        }
        private void In_rb2_CheckedChanged(object sender, System.EventArgs e)
        {
            result_spots_2 = true;
        }

        public bool Result_Spots()
        {
            bool result_spots = false;
            if (result_spots_2)
            {
                result_spots = true;
            }
            return result_spots;
        }
    }
}
