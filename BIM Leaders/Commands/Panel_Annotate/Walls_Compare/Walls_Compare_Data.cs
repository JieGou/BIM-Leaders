using Autodesk.Revit.DB;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="Walls_Compare"/>
    /// </summary>
    public class Walls_Compare_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Walls_Compare_Data"/> class.
        /// </summary>
        public Walls_Compare_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Walls_Compare_Data"/> material name.
        /// </summary>
        public string result_mat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Walls_Compare_Data"/> fill type id.
        /// </summary>
        public ElementId result_fill_id { get; set; }
    }
}
