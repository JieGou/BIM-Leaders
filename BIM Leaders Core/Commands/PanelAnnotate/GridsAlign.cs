using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GridsAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                int count2D = 0;
                int count = 0;

                // Get user provided information from window
                GridsAlignForm form = new GridsAlignForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Collector for data provided in window
                GridsAlignData data = form.DataContext as GridsAlignData;

                // Getting input from user
                bool inputSwitch = data.ResultSwitch;
                bool inputSide1 = data.ResultSide1;
                bool inputSide2 = data.ResultSide2;

                DatumExtentType extentMode = DatumExtentType.ViewSpecific;

                IEnumerable<Grid> grids = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .ToElements()
                    .Cast<Grid>();

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Grids"))
                {
                    trans.Start();

                    (count2D, count) = EditExtents(doc, extentMode, grids, inputSwitch, inputSide1, inputSide2);

                    trans.Commit();
                }

                // Show result
                string text = count == 0
                    ? "No grids aligned"
                    : $"{count2D} grids switched to 2D and aligned.{Environment.NewLine}{count} grids changed bubbles";
                TaskDialog.Show("Grids Align", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Edit grids extents.
        /// </summary>
        /// <returns>Count of grids switched to 2D and count of grids processed.</returns>
        private static (int, int) EditExtents(Document doc, DatumExtentType extentMode, IEnumerable<Grid> grids, bool inputSwitch, bool inputSide1, bool inputSide2)
        {
            int count2D = 0;
            int count = 0;

            View view = doc.ActiveView;

            Curve curve = grids.First().GetCurvesInView(extentMode, view)[0];
            double curve_1_x = curve.GetEndPoint(0).X;
            double curve_1_y = curve.GetEndPoint(0).Y;
            double curve_1_z = curve.GetEndPoint(0).Z;
            double curve_2_x = curve.GetEndPoint(1).X;
            double curve_2_y = curve.GetEndPoint(1).Y;
            double curve_2_z = curve.GetEndPoint(1).Z;

            foreach (Grid grid in grids)
            {
                if (inputSwitch)
                {
                    grid.SetDatumExtentType(DatumEnds.End0, view, extentMode);
                    grid.SetDatumExtentType(DatumEnds.End1, view, extentMode);

                    if (view.ViewType == ViewType.Elevation | view.ViewType == ViewType.Section)
                    {
                        Curve gridCurve = grid.GetCurvesInView(extentMode, view)[0];
                        XYZ point0 = new XYZ(gridCurve.GetEndPoint(0).X, gridCurve.GetEndPoint(0).Y, curve.GetEndPoint(0).Z);
                        XYZ point1 = new XYZ(gridCurve.GetEndPoint(1).X, gridCurve.GetEndPoint(1).Y, curve.GetEndPoint(1).Z);
                        Line line = Line.CreateBound(point0, point1);
                        grid.SetCurveInView(extentMode, view, line);
                    }
                    count2D++;
                }
                if (inputSide1)
                    grid.ShowBubbleInView(DatumEnds.End0, view);
                if (!inputSide1)
                    grid.HideBubbleInView(DatumEnds.End0, view);
                if (inputSide2)
                    grid.ShowBubbleInView(DatumEnds.End1, view);
                if (!inputSide2)
                    grid.HideBubbleInView(DatumEnds.End1, view);
                count++;
            }
            return (count2D, count);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
        }
    }
}
