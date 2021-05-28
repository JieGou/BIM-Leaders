using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Core
{
    public partial class Dimension_Section_Floors_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button_delete = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_exit = new System.Windows.Forms.Button();
            this.in_thickness = new System.Windows.Forms.NumericUpDown();
            this.in_rb = new System.Windows.Forms.GroupBox();
            this.in_rb2 = new System.Windows.Forms.RadioButton();
            this.in_rb1 = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolTip_thickness = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.in_thickness)).BeginInit();
            this.in_rb.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_delete
            // 
            this.button_delete.BackColor = System.Drawing.Color.White;
            this.button_delete.FlatAppearance.BorderSize = 0;
            this.button_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_delete.Font = new System.Drawing.Font("Roboto", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_delete.Location = new System.Drawing.Point(60, 8);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(100, 32);
            this.button_delete.TabIndex = 1;
            this.button_delete.Text = "OK";
            this.button_delete.UseVisualStyleBackColor = false;
            this.button_delete.Click += new System.EventHandler(this.Button_ok_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(15, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Top Floor Max Thickness (cm):";
            this.label1.MouseHover += new System.EventHandler(this.label1_MouseHover);
            // 
            // button_exit
            // 
            this.button_exit.BackColor = System.Drawing.Color.White;
            this.button_exit.FlatAppearance.BorderSize = 0;
            this.button_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_exit.Font = new System.Drawing.Font("Roboto", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button_exit.Location = new System.Drawing.Point(200, 8);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(100, 32);
            this.button_exit.TabIndex = 3;
            this.button_exit.Text = "EXIT";
            this.button_exit.UseVisualStyleBackColor = false;
            this.button_exit.Click += new System.EventHandler(this.Button_exit_Click);
            // 
            // in_thickness
            // 
            this.in_thickness.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.in_thickness.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.in_thickness.Location = new System.Drawing.Point(267, 152);
            this.in_thickness.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.in_thickness.Name = "in_thickness";
            this.in_thickness.Size = new System.Drawing.Size(48, 26);
            this.in_thickness.TabIndex = 7;
            this.in_thickness.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.in_thickness.ValueChanged += new System.EventHandler(this.In_thickness_ValueChanged);
            // 
            // in_rb
            // 
            this.in_rb.Controls.Add(this.in_rb2);
            this.in_rb.Controls.Add(this.in_rb1);
            this.in_rb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.in_rb.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.in_rb.Location = new System.Drawing.Point(18, 54);
            this.in_rb.Name = "in_rb";
            this.in_rb.Size = new System.Drawing.Size(212, 72);
            this.in_rb.TabIndex = 8;
            this.in_rb.TabStop = false;
            this.in_rb.Text = "Annotation Type";
            // 
            // in_rb1
            // 
            this.in_rb1.AutoSize = true;
            this.in_rb1.Checked = true;
            this.in_rb1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.in_rb1.Location = new System.Drawing.Point(6, 19);
            this.in_rb1.Name = "in_rb1";
            this.in_rb1.Size = new System.Drawing.Size(109, 24);
            this.in_rb1.TabIndex = 0;
            this.in_rb1.TabStop = true;
            this.in_rb1.Text = "Dimensions";
            this.in_rb1.UseVisualStyleBackColor = true;
            this.in_rb1.CheckedChanged += new System.EventHandler(this.In_rb1_CheckedChanged);
            // 
            // in_rb2
            // 
            this.in_rb2.AutoSize = true;
            this.in_rb2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.in_rb2.Location = new System.Drawing.Point(6, 42);
            this.in_rb2.Name = "in_rb2";
            this.in_rb2.Size = new System.Drawing.Size(137, 24);
            this.in_rb2.TabIndex = 1;
            this.in_rb2.TabStop = true;
            this.in_rb2.Text = "Spot Elevations";
            this.in_rb2.UseVisualStyleBackColor = true;
            this.in_rb2.CheckedChanged += new System.EventHandler(this.In_rb2_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 48);
            this.panel1.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(51, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(250, 24);
            this.label2.TabIndex = 11;
            this.label2.Text = "Dimension Section Floors";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel2.Controls.Add(this.button_exit);
            this.panel2.Controls.Add(this.button_delete);
            this.panel2.Location = new System.Drawing.Point(0, 195);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(360, 48);
            this.panel2.TabIndex = 10;
            // 
            // toolTip_thickness
            // 
            this.toolTip_thickness.BackColor = System.Drawing.Color.LightGray;
            // 
            // Dimension_Section_Floors_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(360, 240);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.in_rb);
            this.Controls.Add(this.in_thickness);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Roboto", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Dimension_Section_Floors_Form";
            this.ShowIcon = false;
            this.Text = "Dimension Section Floors";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Dimension_Section_Floors_Form_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.in_thickness)).EndInit();
            this.in_rb.ResumeLayout(false);
            this.in_rb.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Label label1;
        private Button button_exit;
        private NumericUpDown in_thickness;
        private GroupBox in_rb;
        private RadioButton in_rb2;
        private RadioButton in_rb1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private ToolTip toolTip_thickness;
        private System.ComponentModel.IContainer components;
        private Label label2;
    }
}