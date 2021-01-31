using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace _BIM_Leaders
{
    public partial class Linetypes_IMPORT_Delete_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.button_delete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_exit = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button_delete
            // 
            this.button_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_delete.Location = new System.Drawing.Point(235, 42);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(75, 23);
            this.button_delete.TabIndex = 1;
            this.button_delete.Text = "Delete";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.Button_delete_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(12, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Linetype Name Contains";
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
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 45);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(204, 20);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "IMPORT";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Linetypes_IMPORT_Delete_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(322, 114);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_delete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Linetypes_IMPORT_Delete_Form";
            this.ShowIcon = false;
            this.Text = "DWG Name Delete";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Linetypes_IMPORT_Delete_Form_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Label label1;
        private Button button_exit;
        private TextBox textBox1;
    }
}