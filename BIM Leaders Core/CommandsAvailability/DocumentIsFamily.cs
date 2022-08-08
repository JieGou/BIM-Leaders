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
}