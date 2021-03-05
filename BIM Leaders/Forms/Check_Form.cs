using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace _BIM_Leaders
{
    public partial class Check_Form : System.Windows.Forms.Form
    {
        public Check_Form()
        {
            InitializeComponent();
        }
        System.Drawing.Point last_point;
        private void Check_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Check_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }

        // New
        string prefix = "PRE_";
        List<bool> categories = Enumerable.Repeat(false, 24).ToList();
        List<bool> model = Enumerable.Repeat(false, 5).ToList();
        List<bool> codes = Enumerable.Repeat(false, 2).ToList();
        int head_height = 210;

        // Get input data
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            prefix = textBox1.Text;
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                button_check.Enabled = false;
            }
            else
            {
                button_check.Enabled = true;
            }
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            // Get all checked categories to true value
            for (int index = 0;  index < checkedListBox1.Items.Count - 1; index++)
            {
                categories[index] = checkedListBox1.Items[index].Equals(true);
            }
        }
        private void checkedListBox2_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            for (int index = 0; index < checkedListBox2.Items.Count - 1; index++)
            {
                model[index] = checkedListBox2.Items[index].Equals(true);
            }
        }
        private void checkedListBox3_SelectedIndexChanged(object sender, ItemCheckEventArgs e)
        {
            for (int index = 0; index < checkedListBox3.Items.Count - 1; index++)
            {
                codes[index] = checkedListBox3.Items[index].Equals(true);
            }
        }
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            head_height = Decimal.ToInt32(numericUpDown1.Value);
        }
        // Buttons actions
        private void Button_Check_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Return results
        public string Result_prefix()
        {
            string result = prefix;
            return result; 
        }
        public List<bool> Result_categories()
        {
            List<bool> result = categories;
            return result;
        }
        public List<bool> Result_model()
        {
            List<bool> result = model;
            return result;
        }
        public List<bool> Result_codes()
        {
            List<bool> result = codes;
            return result;
        }
        public int Result_height()
        {
            int result = head_height;
            return result;
        }
    }
}
