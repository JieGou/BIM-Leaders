using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    public static class ViewUtils
    {
        /// <summary>
        /// Filter for FilteredElementCollector to get elements that only cuts the current view.
        /// </summary>
        /// <returns>ElementIntersectsSolidFilter element.</returns>
        public static ElementIntersectsSolidFilter GetViewCutIntersectFilter(View view)
        {
            // Solid of view section plane for filtering
            IList<CurveLoop> viewCrop = view.GetCropRegionShapeManager().GetCropShape();
            
            // ElementIntersectsSolidFilter has issues with finding intersections
            // if no edges intersects the elements so need to move view boundaries back a little
            Transform transform = Transform.CreateTranslation(view.ViewDirection.Negate());
            foreach (CurveLoop curveLoop in viewCrop)
                curveLoop.Transform(transform);
            
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(viewCrop, view.ViewDirection, 2);

            ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(solid);

            return intersectFilter;
        }
    }
}