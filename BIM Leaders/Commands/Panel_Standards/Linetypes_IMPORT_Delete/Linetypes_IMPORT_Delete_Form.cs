using System.Collections.Generic;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Linetypes delete aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Linetypes_IMPORT_Delete_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Linetypes_IMPORT_Delete_Form"/>
        /// </summary>
        public Linetypes_IMPORT_Delete_Form()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Button_delete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_delete_Click(object sender, System.EventArgs e)
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
        private void Linetypes_IMPORT_Delete_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void DWG_Name_Delete_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Linetypes_IMPORT_Delete_Data GetInformation()
        {
            // Information gathered from window
            var information = new Linetypes_IMPORT_Delete_Data()
            {
                result_name = textBox1.ToString()
            };
            return information;
        }

        string result = "IMPORT";
        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            result = textBox1.ToString();
        }
        public string Result()
        {
            return result; 
        }

    }
}