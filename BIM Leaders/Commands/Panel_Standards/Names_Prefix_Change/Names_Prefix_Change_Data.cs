using System.Collections.Generic;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Information and data model for command <see cref="Names_Prefix_Change"/>
    /// </summary>
    public class Names_Prefix_Change_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Names_Prefix_Change_Data"/> class.
        /// </summary>
        public Names_Prefix_Change_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Names_Prefix_Change_Data"/> old prefix text.
        /// </summary>
        public string result_prefix_old { get; set; }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Names_Prefix_Change_Data"/> new prefix text.
        /// </summary>
        public string result_prefix_new { get; set; }

        /// <summary>
        /// Gets or sets a value indicating <see cref="Names_Prefix_Change_Data"/> categories checked.
        /// </summary>
        public IList<bool> result_categories { get; set; }
    }
}
