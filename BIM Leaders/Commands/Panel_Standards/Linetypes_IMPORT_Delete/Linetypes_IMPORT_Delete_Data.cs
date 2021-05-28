using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="Linetypes_IMPORT_Delete"/>
    /// </summary>
    public class Linetypes_IMPORT_Delete_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Linetypes_IMPORT_Delete_Data"/> class.
        /// </summary>
        public Linetypes_IMPORT_Delete_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating linetypes name.
        /// </summary>
        public string result_name { get; set; }
    }
}
