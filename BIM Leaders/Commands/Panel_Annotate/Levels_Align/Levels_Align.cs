using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Levels_Align : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Collector for data provided in window
            Levels_Align_Data data = new Levels_Align_Data();

            // Get user provided information from window
            using (Levels_Align_Form form = new Levels_Align_Form())
            {
                form.ShowDialog();

                if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    return Result.Cancelled;

                data = form.GetInformation();
            }

            // Getting input from user
            bool condition_switch = data.result_switch;
            bool condition_side_1 = data.result_side;
            bool condition_side_2 = false;
            if (!condition_side_1)
                condition_side_2 = true;
            int count_2D = 0;
            int count = 0;

            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                IEnumerable<Level> levels = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Levels).ToElements().Cast<Level>();

                DatumExtentType extent_mode = DatumExtentType.ViewSpecific;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Levels"))
                {
                    trans.Start();

                    Curve curve = levels.First().GetCurvesInView(extent_mode, view)[0];
                    double curve_1_x = curve.GetEndPoint(0).X;
                    double curve_1_y = curve.GetEndPoint(0).Y;
                    double curve_2_x = curve.GetEndPoint(1).X;
                    double curve_2_y = curve.GetEndPoint(1).Y;

                    foreach (Level l in levels)
                    {
                        if (condition_switch)
                        {
                            l.SetDatumExtentType(DatumEnds.End0, view, extent_mode);
                            l.SetDatumExtentType(DatumEnds.End1, view, extent_mode);

                            if (view.ViewType == ViewType.Elevation | view.ViewType == ViewType.Section)
                            {
                                Curve c = l.GetCurvesInView(extent_mode, view)[0];
                                XYZ p_0 = new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, c.GetEndPoint(0).Z);
                                XYZ p_1 = new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, c.GetEndPoint(1).Z);
                                Line line = Line.CreateBound(p_0, p_1);
                                l.SetCurveInView(extent_mode, view, line);
                            }
                            count_2D++;
                        }
                        if (condition_side_1)
                        {
                            l.ShowBubbleInView(DatumEnds.End0, view);
                        }
                        if (!condition_side_1)
                        {
                            l.HideBubbleInView(DatumEnds.End0, view);
                        }
                        if (condition_side_2)
                        {
                            l.ShowBubbleInView(DatumEnds.End1, view);
                        }
                        if (!condition_side_2)
                        {
                            l.HideBubbleInView(DatumEnds.End1, view);
                        }
                        count++;
                    }
                    trans.Commit();

                    if (count == 0)
                    {
                        TaskDialog.Show("Levels Align", "No levels aligned");
                    }
                    else
                    {
                        TaskDialog.Show("Levels Align", string.Format("{0} levels switched to 2D and aligned {1} levels changed tags", count_2D.ToString(), count.ToString()));
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Levels_Align).Namespace + "." + nameof(Levels_Align);
        }
    }
}
