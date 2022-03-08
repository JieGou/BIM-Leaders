using Autodesk.Revit.UI;

namespace BIM_Leaders
{
    /// <summary>
    /// Startup class for all Revit plugin (described in .addin file)
    /// </summary>
    /// <returns>After initializing on startup returns Result.Succeeded</returns>
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
