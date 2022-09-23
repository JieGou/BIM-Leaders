using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Return true if view type is AreaPlan, CeilingPlan or FloorPlan
    /// </summary>
    public class ViewIsPlan : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;
                if (viewType == ViewType.AreaPlan ||
                    viewType == ViewType.CeilingPlan ||
                    viewType == ViewType.FloorPlan)
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