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
                Document o_doc = applicationData.ActiveUIDocument.Document;

                if (o_doc.IsFamilyDocument)
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
                ViewType v_type = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (v_type == ViewType.Section)
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
            ViewType v_type = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;
            if (v_type == ViewType.FloorPlan)
                return true;
            return false;
        }
    }
}