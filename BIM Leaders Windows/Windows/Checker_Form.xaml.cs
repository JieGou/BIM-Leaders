﻿using System.Windows;
using System.Windows.Input;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class Checker_Form : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Checker_Form"/>
        /// </summary>
        public Checker_Form()
        {
            InitializeComponent();

            DataContext = new Checker_Data();
        }

        /// <summary>
        /// Handles the Click event of the Button_exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_exit_Click(object sender, System.EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Button_ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Button_ok_Click(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Move the window
        private void Form_MouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
