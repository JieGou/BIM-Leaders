using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Return true if view type is AreaPlan, CeilingPlan, Elevation, FloorPlan or Section
    /// </summary>
    public class ViewIsStandard : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                Document doc = applicationData?.ActiveUIDocument?.Document;

                if (doc == null)
                    return false;

                ViewType viewType = doc.ActiveView.ViewType;
                if (
                    viewType == ViewType.AreaPlan ||
                    viewType == ViewType.CeilingPlan ||
                    viewType == ViewType.Elevation ||
                    viewType == ViewType.FloorPlan ||
                    viewType == ViewType.Section
                    )
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