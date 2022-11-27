using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace BIM_Leaders_Windows
{
    public class BaseView : Window
    {
        public BaseView() : base()
        {
        }
        
        private protected void InitializeMaterialDesign()
        {
            // Create dummy objects to force the MaterialDesign assemblies to be loaded
            // from this assembly, which causes the MaterialDesign assemblies to be searched
            // relative to this assembly's path. Otherwise, the MaterialDesign assemblies
            // are searched relative to Eclipse's path, so they're not found.
            Card card = new Card();
            Hue hue = new Hue("Dummy", Colors.Black, Colors.White);
        }

        private protected void ButtonExitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Move the window
        private protected void FormMouseMove(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}