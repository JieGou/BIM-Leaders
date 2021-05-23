using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BIM_Leaders_Core
{
    public partial class DWG_Name_Delete_Form : System.Windows.Forms.Form
    {
        public DWG_Name_Delete_Form(List<ImportInstance> imports)
        {
            InitializeComponent();

            foreach(ImportInstance i in imports)
            {
                DWG_Name_Delete_List.Items.Add(i.Category.Name);
            }
        }
        System.Drawing.Point last_point;
        private void DWG_Name_Delete_Form_MouseMove(object sender, MouseEventArgs e)
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
        string result = "";
        public string Result()
        {
            string result = DWG_Name_Delete_List.SelectedItem.ToString();
            return result; 
        }
    }
}
