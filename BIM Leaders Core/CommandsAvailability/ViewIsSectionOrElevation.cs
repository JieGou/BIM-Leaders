using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Return true if view type is Section
    /// </summary>
    public class ViewIsSectionOrElevation : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (viewType == ViewType.Section ||
                    viewType == ViewType.Elevation)
                    return true;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}