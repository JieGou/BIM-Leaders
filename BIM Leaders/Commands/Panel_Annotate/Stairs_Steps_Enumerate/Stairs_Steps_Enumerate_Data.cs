using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="Stairs_Steps_Enumerate"/>
    /// </summary>
    public class Stairs_Steps_Enumerate_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Stairs_Steps_Enumerate_Data"/> class.
        /// </summary>
        public Stairs_Steps_Enumerate_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Stairs_Steps_Enumerate_Data"/> side chosen.
        /// </summary>
        /// /// /// <value>
        ///     <c>true</c> if right side is chosen, <c>false</c> if left side is chosen.
        /// </value>
        public bool result_side_right { get; set; }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Stairs_Steps_Enumerate_Data"/> start number.
        /// </summary>
        public decimal result_number { get; set; }
    }
}
