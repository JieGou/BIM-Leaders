using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Core
{
    public partial class Check_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.button_check = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_exit = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.Standards = new System.Windows.Forms.GroupBox();
            this.Model = new System.Windows.Forms.GroupBox();
            this.checkedListBox2 = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.Codes = new System.Windows.Forms.GroupBox();
            this.checkedListBox3 = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.Standards.SuspendLayout();
            this.Model.SuspendLayout();
            this.Codes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // button_check
            // 
            this.button_check.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button_check.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_check.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_check.Location = new System.Drawing.Point(713, 414);
            this.button_check.Name = "button_check";
            this.button_check.Size = new System.Drawing.Size(75, 23);
            this.button_check.TabIndex = 1;
            this.button_check.Text = "Check";
            this.button_check.UseVisualStyleBackColor = true;
            this.button_check.Click += new System.EventHandler(this.Button_Check_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Names Prefix:";
            // 
            // button_exit
            // 
            this.button_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_exit.Location = new System.Drawing.Point(713, 443);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(75, 23);
            this.button_exit.TabIndex = 3;
            this.button_exit.Text = "Exit";
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.Button_exit_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(9, 40);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(80, 20);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "PRE_";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label3.Location = new System.Drawing.Point(6, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Categories";
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.AccessibleName = "";
            this.checkedListBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Items.AddRange(new object[] {
            "Area Schemes",
            "Browser Organization",
            "Building Pad Types",
            "Ceiling Types",
            "Curtain System Types",
            "Dimension Types",
            "Families",
            "Filled Region Types",
            "Grid Types",
            "Group Types",
            "Level Types",
            "Line Patterns",
            "Materials",
            "Panel Types",
            "Railing Types",
            "Roof Types",
            "Spot Dimension Types",
            "Stair Types",
            "Stair Landing Types",
            "Stair Run Types",
            "Text Note Types",
            "Views",
            "Wall Types",
            "Wall Foundation Types"});
            this.checkedListBox1.Location = new System.Drawing.Point(9, 92);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(201, 362);
            this.checkedListBox1.TabIndex = 8;
            this.checkedListBox1.Tag = "";
            this.checkedListBox1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox1_SelectedIndexChanged);
            // 
            // Standards
            // 
            this.Standards.Controls.Add(this.label1);
            this.Standards.Controls.Add(this.checkedListBox1);
            this.Standards.Controls.Add(this.textBox1);
            this.Standards.Controls.Add(this.label3);
            this.Standards.Location = new System.Drawing.Point(12, 12);
            this.Standards.Name = "Standards";
            this.Standards.Size = new System.Drawing.Size(219, 466);
            this.Standards.TabIndex = 9;
            this.Standards.TabStop = false;
            this.Standards.Text = "Standards";
            // 
            // Model
            // 
            this.Model.Controls.Add(this.checkedListBox2);
            this.Model.Controls.Add(this.label4);
            this.Model.Location = new System.Drawing.Point(237, 12);
            this.Model.Name = "Model";
            this.Model.Size = new System.Drawing.Size(219, 466);
            this.Model.TabIndex = 10;
            this.Model.TabStop = false;
            this.Model.Text = "Model";
            // 
            // checkedListBox2
            // 
            this.checkedListBox2.AccessibleName = "";
            this.checkedListBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBox2.FormattingEnabled = true;
            this.checkedListBox2.Items.AddRange(new object[] {
            "Groups",
            "Unused Linestyles",
            "Rooms",
            "Warnings",
            "Exterior Walls"});
            this.checkedListBox2.Location = new System.Drawing.Point(9, 92);
            this.checkedListBox2.Name = "checkedListBox2";
            this.checkedListBox2.Size = new System.Drawing.Size(201, 362);
            this.checkedListBox2.TabIndex = 8;
            this.checkedListBox2.Tag = "";
            this.checkedListBox2.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox2_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label4.Location = new System.Drawing.Point(6, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Checks";
            // 
            // Codes
            // 
            this.Codes.Controls.Add(this.label5);
            this.Codes.Controls.Add(this.numericUpDown1);
            this.Codes.Controls.Add(this.checkedListBox3);
            this.Codes.Controls.Add(this.label2);
            this.Codes.Location = new System.Drawing.Point(462, 12);
            this.Codes.Name = "Codes";
            this.Codes.Size = new System.Drawing.Size(219, 466);
            this.Codes.TabIndex = 11;
            this.Codes.TabStop = false;
            this.Codes.Text = "Codes";
            // 
            // checkedListBox3
            // 
            this.checkedListBox3.AccessibleName = "";
            this.checkedListBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedListBox3.FormattingEnabled = true;
            this.checkedListBox3.Items.AddRange(new object[] {
            "Stairs Formula",
            "Stairs Head Height"});
            this.checkedListBox3.Location = new System.Drawing.Point(9, 92);
            this.checkedListBox3.Name = "checkedListBox3";
            this.checkedListBox3.Size = new System.Drawing.Size(201, 362);
            this.checkedListBox3.TabIndex = 8;
            this.checkedListBox3.Tag = "";
            this.checkedListBox3.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox3_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label2.Location = new System.Drawing.Point(6, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Checks";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDown1.Location = new System.Drawing.Point(9, 40);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            300,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            210,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(77, 20);
            this.numericUpDown1.TabIndex = 12;
            this.numericUpDown1.Value = new decimal(new int[] {
            210,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label5.Location = new System.Drawing.Point(6, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Stairs Head Height (cm):";
            // 
            // Check_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(800, 540);
            this.Controls.Add(this.Codes);
            this.Controls.Add(this.Model);
            this.Controls.Add(this.Standards);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_check);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Check_Form";
            this.ShowIcon = false;
            this.Text = "Names Prefix Change";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Check_Form_MouseMove);
            this.Standards.ResumeLayout(false);
            this.Standards.PerformLayout();
            this.Model.ResumeLayout(false);
            this.Model.PerformLayout();
            this.Codes.ResumeLayout(false);
            this.Codes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }
        private System.Windows.Forms.Button button_check;
        private System.Windows.Forms.Label label1;
        private Button button_exit;
        private TextBox textBox1;
        private Label label3;
        private CheckedListBox checkedListBox1;
        private GroupBox Standards;
        private GroupBox Model;
        private CheckedListBox checkedListBox2;
        private Label label4;
        private GroupBox Codes;
        private CheckedListBox checkedListBox3;
        private Label label2;
        private Label label5;
        private NumericUpDown numericUpDown1;
    }
}