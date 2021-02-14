using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _BIM_Leaders
{
    public partial class Names_Prefix_Change_Form : System.Windows.Forms.Form
    {
        public Names_Prefix_Change_Form()
        {
            InitializeComponent();
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

        // New
        string prefix_old = "OLD";
        string prefix_new = "NEW";
        List<bool> categories = Enumerable.Repeat(false, 24).ToList();

        // Get input data
        private void textBox1_TextChanged(object sender, EventArgs e)
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
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            prefix_new = textBox2.Text;
            /*
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                button_rename.Enabled = false;
            }
            else
            {
                button_rename.Enabled = true;
            }
            */
            button_rename.Enabled = !string.IsNullOrWhiteSpace(textBox2.Text);
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            // Get all checked categories to true value
            for (int index = 0;  index < checkedListBox1.Items.Count - 1; index++)
            {
                //int index = checkedListBox1.Items.IndexOf(item);
                /*
                if (checkedListBox1.Items[index].Equals(true))
                {
                    categories[index] = true;
                }
                else
                {
                    categories[index] = false;
                }
                */
                categories[index] = checkedListBox1.Items[index].Equals(true);
            }
        }

        // Buttons actions
        private void Button_rename_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
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
