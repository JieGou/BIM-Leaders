using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Return true if document type is FamilyDocument
    /// </summary>
    public class DocumentIsFamily : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                Document doc = applicationData.ActiveUIDocument.Document;

                if (doc.IsFamilyDocument)
                    return true;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Return true if view type is Section
    /// </summary>
    public class ViewIsSection : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (viewType == ViewType.Section)
                    return true;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Return true if view type is Section
    /// </summary>
    public class ViewIsSectionOrElevation : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (viewType == ViewType.Section || viewType == ViewType.Elevation)
                    return true;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Return true if view type is FloorPlan
    /// </summary>
    public class ViewIsPlan : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            ViewType viewType = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;
            if (viewType == ViewType.FloorPlan)
                return true;
            return false;
        }
    }
}