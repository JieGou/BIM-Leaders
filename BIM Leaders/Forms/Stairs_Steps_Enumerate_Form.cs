using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _BIM_Leaders
{
    public partial class Stairs_Steps_Enumerate_Form : System.Windows.Forms.Form
    {
        public Stairs_Steps_Enumerate_Form()
        {
            InitializeComponent();
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
        private void Button_ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        bool result_side_right = false;
        bool result_side_left = false;
        decimal result_number = 1;

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            result_number = numericUpDown1.Value;
        }
        public decimal Result_Number()
        {
            return result_number;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            result_side_right = true;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            result_side_left = true;
        }

        public bool Result_Spots()
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
