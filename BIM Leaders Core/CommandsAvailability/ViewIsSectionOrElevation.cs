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
                Document doc = applicationData?.ActiveUIDocument?.Document;

                if (doc == null)
                    return false;

                ViewType viewType = doc.ActiveView.ViewType;

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