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
    public partial class Grids_Align_Form : System.Windows.Forms.Form
    {
        public Grids_Align_Form()
        {
            InitializeComponent();
        }
        System.Drawing.Point last_point;
        private void Grids_Align_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Grids_Align_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }
        private void Button_ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        private void Button_exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        bool condition_switch = true;
        bool condition_side_1 = true;
        bool condition_side_2 = false;

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(!checkBox1.Checked)
            {
                condition_switch = false;
            }
        }

        public bool Result_Switch()
        {
            return condition_switch;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if(radioButton1.Checked)
            {
                condition_side_1 = true;
            }
            else
            {
                condition_side_1 = false;
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                condition_side_2 = true;
            }
            else
            {
                condition_side_2 = false;
            }
        }

        public bool Result_Side()
        {
            bool condition_side = false;
            if (condition_side_1)
            {
                condition_side = true;
            }
            return condition_side;
        }
    }
}
