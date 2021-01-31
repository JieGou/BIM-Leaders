using System.Windows.Forms;

namespace _BIM_Leaders
{
    public partial class Levels_Align_Form : System.Windows.Forms.Form
    {
        private void InitializeComponent()
        {
            this.button_delete = new System.Windows.Forms.Button();
            this.button_exit = new System.Windows.Forms.Button();
            this.radiobuttons = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.radiobuttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_delete
            // 
            this.button_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.button_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_delete.Location = new System.Drawing.Point(235, 59);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(75, 23);
            this.button_delete.TabIndex = 1;
            this.button_delete.Text = "Ok";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.Button_ok_Click);
            // 
            // button_exit
            // 
            this.button_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_exit.Location = new System.Drawing.Point(235, 88);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(75, 23);
            this.button_exit.TabIndex = 3;
            this.button_exit.Text = "Exit";
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.Button_exit_Click);
            // 
            // radiobuttons
            // 
            this.radiobuttons.Controls.Add(this.radioButton2);
            this.radiobuttons.Controls.Add(this.radioButton1);
            this.radiobuttons.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radiobuttons.Location = new System.Drawing.Point(12, 12);
            this.radiobuttons.Name = "radiobuttons";
            this.radiobuttons.Size = new System.Drawing.Size(212, 72);
            this.radiobuttons.TabIndex = 8;
            this.radiobuttons.TabStop = false;
            this.radiobuttons.Text = "Tags Side";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButton2.Location = new System.Drawing.Point(6, 42);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(54, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Side 2";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(54, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Side 1";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(18, 92);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(87, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Switch to 2D";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Levels_Align_Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(322, 139);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.radiobuttons);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_delete);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Levels_Align_Form";
            this.ShowIcon = false;
            this.Text = "Levels Align";
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Levels_Align_Form_MouseMove);
            this.radiobuttons.ResumeLayout(false);
            this.radiobuttons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        private System.Windows.Forms.Button button_delete;
        private Button button_exit;
        private GroupBox radiobuttons;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private CheckBox checkBox1;
    }
}