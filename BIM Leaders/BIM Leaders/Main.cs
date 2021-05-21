using Autodesk.Revit.UI;

namespace BIM_Leaders
{
    public class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // Initialize whole plugin's user interface
            var ui = new SetupInterface();
            ui.Initialize(application);

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
        return Result.Succeeded;
        }
    }
}
