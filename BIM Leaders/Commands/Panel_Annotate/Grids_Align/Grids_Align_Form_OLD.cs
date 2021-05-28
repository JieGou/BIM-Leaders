using System.Collections.Generic;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Align grids aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Grids_Align_Form_OLD : System.Windows.Forms.Form
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Grids_Align_Form"/>
        /// </summary>
        public Grids_Align_Form_OLD()
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

        System.Drawing.Point last_point;
        private void Grids_Align_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Grids_Align_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Grids_Align_Data GetInformation()
        {
            // Information gathered from window
            var information = new Grids_Align_Data()
            {
                result_side = radioButton1.Checked,
                result_switch = checkBox1.Checked
            };
            return information;
        }

        bool condition_switch = true;
        bool condition_side_1 = true;
        bool condition_side_2 = false;

        private void checkBox1_CheckedChanged(object sender, System.EventArgs e)
        {
            if(!checkBox1.Checked)
            {
                condition_switch = false;
            }
        }

        public bool Result_Switch()
        {
            return condition_switch;
        }

        private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
        {
            if(radioButton1.Checked)
            {
                condition_side_1 = true;
            }
            else
            {
                condition_side_1 = false;
            }
        }
        private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
        {
            if (radioButton2.Checked)
            {
                condition_side_2 = true;
            }
            else
            {
                condition_side_2 = false;
            }
        }

        public bool Result_Side()
        {
            bool condition_side = false;
            if (condition_side_1)
            {
                condition_side = true;
            }
            return condition_side;
        }
    }
}
