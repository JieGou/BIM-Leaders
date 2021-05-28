using Autodesk.Revit.DB;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="DWG_Name_Delete"/>
    /// </summary>
    public class DWG_Name_Delete_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="DWG_Name_Delete_Data"/> class.
        /// </summary>
        public DWG_Name_Delete_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating DWG name.
        /// </summary>
        public ElementId result_dwg { get; set; }
    }
}
