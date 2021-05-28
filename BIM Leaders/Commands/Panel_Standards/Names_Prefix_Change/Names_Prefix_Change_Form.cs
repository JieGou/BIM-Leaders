using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Names prefix change aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form"/>
    public partial class Names_Prefix_Change_Form : System.Windows.Forms.Form
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Names_Prefix_Change_Form"/>
        /// </summary>
        public Names_Prefix_Change_Form()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the Button_rename control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_rename_Click(object sender, System.EventArgs e)
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
        private void Names_Prefix_Change_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Names_Prefix_Change_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        /// <summary>
        /// Gets the information from user.
        /// </summary>
        /// <returns></returns>
        public Names_Prefix_Change_Data GetInformation()
        {
            // Information gathered from window
            var information = new Names_Prefix_Change_Data()
            {
                result_prefix_old = textBox1.Text,
                result_prefix_new = textBox2.Text,
                result_categories = checkedListBox1.SelectedIndices as IList<bool>
            };
            return information;
        }

        // New
        string prefix_old = "OLD";
        string prefix_new = "NEW";
        List<bool> categories = Enumerable.Repeat(false, 24).ToList();

        // Get input data
        private void textBox1_TextChanged(object sender, System.EventArgs e)
        {
            prefix_old = textBox1.Text;
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                button_rename.Enabled = false;
            }
            else
            {
                button_rename.Enabled = true;
            }
        }
        private void textBox2_TextChanged(object sender, System.EventArgs e)
        {
            prefix_new = textBox2.Text;
            button_rename.Enabled = !string.IsNullOrWhiteSpace(textBox2.Text);
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            // Get all checked categories to true value
            for (int index = 0;  index < checkedListBox1.Items.Count - 1; index++)
            {
                categories[index] = checkedListBox1.Items[index].Equals(true);
            }
        }

        // Return results
        public string Result_prefix_old()
        {
            string result = prefix_old;
            return result; 
        }
        public string Result_prefix_new()
        {
            string result = prefix_new;
            return result;
        }
        public List<bool> Result_categories()
        {
            List<bool> result = categories;
            return result;
        }
    }
}
