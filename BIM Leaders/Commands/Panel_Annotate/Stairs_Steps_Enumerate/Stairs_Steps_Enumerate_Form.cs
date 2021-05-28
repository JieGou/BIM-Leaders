﻿using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Enumerate steps aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Stairs_Steps_Enumerate_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Stairs_Steps_Enumerate_Form"/>
        /// </summary>
        public Stairs_Steps_Enumerate_Form()
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
        private void Stairs_Steps_Enumerate_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Stairs_Steps_Enumerate_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Stairs_Steps_Enumerate_Data GetInformation()
        {
            // Information gathered from window
            var information = new Stairs_Steps_Enumerate_Data()
            {
                result_side_right = radioButton1.Checked,
                result_number = numericUpDown1.Value
            };
            return information;
        }

        bool result_side_right = false;
        bool result_side_left = false;
        decimal result_number = 1;

        private void numericUpDown1_ValueChanged(object sender, System.EventArgs e)
        {
            result_number = numericUpDown1.Value;
        }
        public decimal Result_Number()
        {
            return result_number;
        }

        private void radioButton1_CheckedChanged(object sender, System.EventArgs e)
        {
            result_side_right = true;
        }
        private void radioButton2_CheckedChanged(object sender, System.EventArgs e)
        {
            result_side_left = true;
        }

        public bool Result_Side()
        {
            bool result_side = false;
            if (result_side_right)
            {
                result_side = true;
            }
            return result_side;
        }
    }
}