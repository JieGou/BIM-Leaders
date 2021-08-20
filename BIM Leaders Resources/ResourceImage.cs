using System.IO;
using System.Windows.Media.Imaging;

namespace BIM_Leaders_Resources
{
    // Gets the embedded resource image from the BIM Leaders Resources assembly based on user provided file name with extension
    public static class ResourceImage
    {
        // Gets the icon image from the users assembly
        public static BitmapImage GetIcon(string name)
        {
            // Create the resource reader
            var stream = ResourceAssembly.GetAssembly().GetManifestResourceStream(ResourceAssembly.GetNamespace() + "Images.Icons." + name);

            var image = new BitmapImage();

            // Construct and return image
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();

            // Return constructed BitmapImage
            return image;
        }
    }
}
