using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    internal static class DatumPlaneUtils
    {
        internal static void SetDatumPlanes(Document doc, Type type, bool inputSwitch2D, bool inputSwitch3D, bool inputSide1, bool inputSide2, ref int count)
        {
            View view = doc.ActiveView;

            IEnumerable<DatumPlane> datumPlanes = new FilteredElementCollector(doc, view.Id)
                    .OfClass(type)
                    .ToElements()
                    .Cast<DatumPlane>();

            Curve curve = datumPlanes.FirstOrDefault().GetCurvesInView(DatumExtentType.ViewSpecific, view)[0];

            foreach (DatumPlane datumPlane in datumPlanes)
            {
                if (inputSwitch2D)
                    SetDatumEnds2D(view, datumPlane, curve);
                else if (inputSwitch3D)
                    SetDatumEnds3D(view, datumPlane);
                SetDatumBubbles(view, datumPlane, inputSide1, inputSide2);

                count++;
            }
        }

        private static void SetDatumEnds2D(View view, DatumPlane datum, Curve curve)
        {
            DatumExtentType extentMode = DatumExtentType.ViewSpecific;

            datum.SetDatumExtentType(DatumEnds.End0, view, extentMode);
            datum.SetDatumExtentType(DatumEnds.End1, view, extentMode);

            if (view.ViewType == ViewType.Elevation || view.ViewType == ViewType.Section)
            {
                Curve currentCurve = datum.GetCurvesInView(extentMode, view)[0];

                XYZ point0 = currentCurve.Project(curve.GetEndPoint(0)).XYZPoint;
                XYZ point1 = currentCurve.Project(curve.GetEndPoint(1)).XYZPoint;

                Line line = Line.CreateBound(point0, point1);
                
                datum.SetCurveInView(extentMode, view, line);
            }
        }

        private static void SetDatumEnds3D(View view, DatumPlane datum)
        {
            DatumExtentType extentMode = DatumExtentType.Model;

            datum.SetDatumExtentType(DatumEnds.End0, view, extentMode);
            datum.SetDatumExtentType(DatumEnds.End1, view, extentMode);
        }

        private static void SetDatumBubbles(View view, DatumPlane datum, bool setSide1, bool setSide2)
        {
            if (setSide1)
                datum.ShowBubbleInView(DatumEnds.End0, view);
            else
                datum.HideBubbleInView(DatumEnds.End0, view);
            if (setSide2)
                datum.ShowBubbleInView(DatumEnds.End1, view);
            else
                datum.HideBubbleInView(DatumEnds.End1, view);
        }
    }
}
