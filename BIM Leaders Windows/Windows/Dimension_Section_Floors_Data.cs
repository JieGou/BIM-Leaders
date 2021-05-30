using Autodesk.Revit.UI;

namespace BIM_Leaders_Windows
{
    /// <summary>
    /// Information and data model for command <see cref="Dimension_Section_Floors"/>
    /// </summary>
    public class Dimension_Section_Floors_Data
    {
        /// <summary>
        /// Default constructor
        /// Initializing a new instance of the <see cref="Dimension_Section_Floors_Data"/> class.
        /// </summary>
        public Dimension_Section_Floors_Data()
        {

        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Dimension_Section_Floors_Data"/> is Spot Elevations or Dimensions.
        /// </summary>
        /// <value>
        ///     <c>true</c> if Spot Elevations are chosen, if Dimensions are chothen, then <c>false</c>.
        /// </value>
        public bool result_spots { get; set; }

        /// <summary>
        /// Gets or sets a value of floors thickness when this <see cref="Dimension_Section_Floors_Data"/> will make text move adjustments.
        /// </summary>
        public double result_thickness { get; set; }
    }
}
