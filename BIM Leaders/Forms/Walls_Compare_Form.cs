using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace _BIM_Leaders
{
    public partial class Walls_Compare_Form : System.Windows.Forms.Form
    {
        public Walls_Compare_Form(List<string> fills)
        {
            InitializeComponent();

            foreach(string i in fills)
            {
                Walls_Compare_Form_List_Fills.Items.Add(i);
            }
        }
        System.Drawing.Point last_point;
        private void Walls_Compare_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Walls_Compare_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button_compare.Enabled = !string.IsNullOrWhiteSpace(textBox1.Text);
        }
        private void Button_delete_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        string result = "";
        public string Result_Material()
        {
            string result_mat = textBox1.Text.ToString();
            return result_mat; 
        }
        public string Result_Fill()
        {
            string result_fill = Walls_Compare_Form_List_Fills.SelectedItem.ToString();
            return result_fill;
        }
    }
}
