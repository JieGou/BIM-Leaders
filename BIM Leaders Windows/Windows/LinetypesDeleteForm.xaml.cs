﻿using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class LinetypesDeleteForm : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="Linetypes_IMPORT_Delete_Form"/>
        /// </summary>
        public LinetypesDeleteForm()
        {
            InitializeMaterialDesign();
            InitializeComponent();

            DataContext = new LinetypesDeleteData();
        }

        private void InitializeMaterialDesign()
        {
            // Create dummy objects to force the MaterialDesign assemblies to be loaded
            // from this assembly, which causes the MaterialDesign assemblies to be searched
            // relative to this assembly's path. Otherwise, the MaterialDesign assemblies
            // are searched relative to Eclipse's path, so they're not found.
            Card card = new Card();
            Hue hue = new Hue("Dummy", Colors.Black, Colors.White);
        }

        /// <summary>
        /// Handles the Click event of the Button_exit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonExitClick(object sender, System.EventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Button_ok control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ButtonOkClick(object sender, System.EventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // Move the window
        private void FormMouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}