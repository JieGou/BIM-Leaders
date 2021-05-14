using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace _BIM_Leaders
{
    public partial class Walls_Compare_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.Walls_Compare_Form_List_Fills = new System.Windows.Forms.ComboBox();
            this.button_compare = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_exit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Walls_Compare_Form_List_Fills
            // 
            this.Walls_Compare_Form_List_Fills.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Walls_Compare_Form_List_Fills.FormattingEnabled = true;
            this.Walls_Compare_Form_List_Fills.Location = new System.Drawing.Point(15, 81);
            this.Walls_Compare_Form_List_Fills.Name = "Walls_Compare_Form_List_Fills";
            this.Walls_Compare_Form_List_Fills.Size = new System.Drawing.Size(204, 21);
            this.Walls_Compare_Form_List_Fills.TabIndex = 0;
            // 
            // button_compare
            // 
            this.button_compare.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button_compare.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_compare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_compare.Location = new System.Drawing.Point(235, 42);
            this.button_compare.Name = "button_compare";
            this.button_compare.Size = new System.Drawing.Size(75, 23);
            this.button_compare.TabIndex = 1;
            this.button_compare.Text = "Compare";
            this.button_compare.UseVisualStyleBackColor = true;
            this.button_compare.Click += new System.EventHandler(this.Button_delete_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Material Name";
            // 
            // button_exit
            // 
            this.button_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_exit.Location = new System.Drawing.Point(235, 71);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(75, 23);
            this.button_exit.TabIndex = 3;
            this.button_exit.Text = "Exit";
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.Button_exit_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Fill Type";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 37);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(204, 20);
            this.textBox1.TabIndex = 5;
            this.textBox1.Text = "Block";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Walls_Compare_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(322, 114);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_compare);
            this.Controls.Add(this.Walls_Compare_Form_List_Fills);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Walls_Compare_Form";
            this.ShowIcon = false;
            this.Text = "Walls Compare";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Walls_Compare_Form_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ComboBox Walls_Compare_Form_List_Fills;
        private System.Windows.Forms.Button button_compare;
        private System.Windows.Forms.Label label1;
        private Button button_exit;
        private Label label2;
        private TextBox textBox1;
    }
}