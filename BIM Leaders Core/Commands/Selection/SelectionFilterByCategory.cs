﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Selection filter based on the user provided category name.
    /// </summary>
    /// <seealso cref="Autodesk.Revit.UI.Selection.ISelectionFilter"/>
    public class SelectionFilterByCategory : ISelectionFilter
    {
        // Private variable that holds category name
        private string mCategory = "";

        /// <summary>
        /// Default constructor.
        /// Initializes a new instance of the <see cref="SelectionFilterByCategory"/>
        /// </summary>
        /// <param name="category">The category of element, such as Walls, Floors,..</param>
        public SelectionFilterByCategory(string category)
        {
            mCategory = category;
        }

        /// <summary>
        /// Allows the element selection if provided category is equal to selected one.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool AllowElement(Element element)
        {
            if (element.Category.Name == mCategory)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Allows the reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}