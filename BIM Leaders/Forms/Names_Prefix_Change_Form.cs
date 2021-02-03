using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
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
        string prefix_old = "";
        string prefix_new = "";
        List<bool> categories = new List<bool>();

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
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                button_rename.Enabled = false;
            }
            else
            {
                button_rename.Enabled = true;
            }
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            categories.Add(checkedListBox1.GetItemChecked(0));
            categories.Add(checkedListBox1.GetItemChecked(1));
            categories.Add(checkedListBox1.GetItemChecked(2));
            categories.Add(checkedListBox1.GetItemChecked(3));
            categories.Add(checkedListBox1.GetItemChecked(4));
            categories.Add(checkedListBox1.GetItemChecked(5));
            categories.Add(checkedListBox1.GetItemChecked(6));
            categories.Add(checkedListBox1.GetItemChecked(7));
            categories.Add(checkedListBox1.GetItemChecked(8));
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
