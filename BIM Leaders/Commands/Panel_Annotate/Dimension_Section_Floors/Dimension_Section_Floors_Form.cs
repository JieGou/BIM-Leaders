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

namespace BIM_Leaders_Core
{
    public partial class Dimension_Section_Floors_Form : System.Windows.Forms.Form
    {
        public Dimension_Section_Floors_Form()
        {
            InitializeComponent();
        }
        System.Drawing.Point last_point;
        private void Dimension_Section_Floors_Form_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Left += e.X - last_point.X - 161;
                this.Top += e.Y - last_point.Y - 57;
            }
        }
        private void Dimension_Section_Floors_Form_MouseDown(object sender, MouseEventArgs e)
        {
            last_point = new System.Drawing.Point(e.X,  e.Y);
        }
        // Tooltip
        private void label1_MouseHover(object sender, EventArgs e)
        {
            toolTip_thickness.Show("From 0 to 50 cm", label1);
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
        bool result_spots_1 = false;
        bool result_spots_2 = false;
        decimal result_thickness = 10;

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            result_thickness = numericUpDown1.Value;
        }
        public decimal Result_Thickness()
        {
            return result_thickness;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            result_spots_1 = true;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            result_spots_2 = true;
        }

        public bool Result_Spots()
        {
            bool result_spots = false;
            if (result_spots_2)
            {
                result_spots = true;
            }
            return result_spots;
        }
    }
}
