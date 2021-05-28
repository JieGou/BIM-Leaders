using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="Levels_Align"/>
    /// </summary>
    public class Levels_Align_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Levels_Align_Data"/> class.
        /// </summary>
        public Levels_Align_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating picked side for <see cref="Levels_Align_Data"/> annotations.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if side 1 is chosen, if side 2 is chosen, then <c>false</c>.
        /// </value>
        public bool result_side { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Levels_Align_Data"/> chosen Switch to 2D.
        /// </summary>
        /// /// <value>
        ///     <c>true</c> if Switch to 2D is chosen, if not, then <c>false</c>.
        /// </value>
        public bool result_switch { get; set; }
    }
}
