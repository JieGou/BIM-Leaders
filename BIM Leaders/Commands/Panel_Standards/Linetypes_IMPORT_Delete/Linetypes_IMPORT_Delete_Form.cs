using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    public partial class Linetypes_IMPORT_Delete_Form : System.Windows.Forms.Form
    {
        public Linetypes_IMPORT_Delete_Form()
        {
            InitializeComponent();
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
        private void Button_delete_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        string result = "IMPORT";
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            result = textBox1.ToString();
        }
        public string Result()
        {
            return result; 
        }

    }
}