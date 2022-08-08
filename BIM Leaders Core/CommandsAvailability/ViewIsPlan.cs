using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Return true if view type is AreaPlan, CeilingPlan or FloorPlan
    /// </summary>
    public class ViewIsPlan : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;
            if (viewType == ViewType.AreaPlan || viewType == ViewType.CeilingPlan || viewType == ViewType.FloorPlan)
                return true;
            return false;
        }
    }
}