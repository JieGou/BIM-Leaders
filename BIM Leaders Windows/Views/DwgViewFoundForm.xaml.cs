﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Dimension floors aquisition form.
    /// </summary>
    /// <seealso cref="System.Windows.Window"/>
    public partial class DwgViewFoundForm : Window
    {
        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="DwgViewFoundForm"/>
        /// </summary>
        public DwgViewFoundForm(DwgViewFoundVM viewModel)
        {
            InitializeMaterialDesign();
            InitializeComponent();

            if (viewModel.CloseAction == null)
                viewModel.CloseAction = new Action(() => this.Close());
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

        private void ButtonExitClick(object sender, RoutedEventArgs e)
        {
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
