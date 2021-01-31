using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace _BIM_Leaders
{
    public partial class DWG_Name_Delete_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.DWG_Name_Delete_List = new System.Windows.Forms.ComboBox();
            this.button_delete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_exit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DWG_Name_Delete_List
            // 
            this.DWG_Name_Delete_List.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DWG_Name_Delete_List.FormattingEnabled = true;
            this.DWG_Name_Delete_List.Location = new System.Drawing.Point(12, 44);
            this.DWG_Name_Delete_List.Name = "DWG_Name_Delete_List";
            this.DWG_Name_Delete_List.Size = new System.Drawing.Size(204, 21);
            this.DWG_Name_Delete_List.TabIndex = 0;
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
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "DWG Name";
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
            // DWG_Name_Delete_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(322, 114);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.DWG_Name_Delete_List);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "DWG_Name_Delete_Form";
            this.ShowIcon = false;
            this.Text = "DWG Name Delete";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DWG_Name_Delete_Form_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ComboBox DWG_Name_Delete_List;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Label label1;
        private Button button_exit;
    }
}