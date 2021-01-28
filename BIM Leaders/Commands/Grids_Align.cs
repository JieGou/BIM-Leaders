using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Grids_Align : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Getting input from user
            bool condition_switch = false;
            bool condition_side_1 = false;
            bool condition_side_2 = false;
            using (Grids_Align_Form form = new Grids_Align_Form())
            {
                form.ShowDialog();

                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    condition_switch = form.Result_Switch();
                    if(form.Result_Side())
                    {
                        condition_side_1 = true;
                        condition_side_2 = false;
                    }
                    else
                    {
                        condition_side_1 = false;
                        condition_side_2 = true;
                    }
                }
            }

            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                IEnumerable<Grid> grids = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Grids).ToElements().Cast<Grid>();

                DatumExtentType extent_mode = DatumExtentType.ViewSpecific;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Grids"))
                {
                    trans.Start();

                    Curve curve = grids.First().GetCurvesInView(extent_mode, view)[0];
                    double curve_1_x = curve.GetEndPoint(0).X;
                    double curve_1_y = curve.GetEndPoint(0).Y;
                    double curve_1_z = curve.GetEndPoint(0).Z;
                    double curve_2_x = curve.GetEndPoint(1).X;
                    double curve_2_y = curve.GetEndPoint(1).Y;
                    double curve_2_z = curve.GetEndPoint(1).Z;

                    int count_2D = 0;
                    int count = 0;

                    foreach (Grid g in grids)
                    {
                        if(condition_switch)
                        {
                            g.SetDatumExtentType(DatumEnds.End0, view, extent_mode);
                            g.SetDatumExtentType(DatumEnds.End1, view, extent_mode);

                            if (view.ViewType == ViewType.Elevation | view.ViewType == ViewType.Section)
                            {
                                Curve c = g.GetCurvesInView(extent_mode, view)[0];
                                XYZ p_0 = new XYZ(c.GetEndPoint(0).X, c.GetEndPoint(0).Y, curve.GetEndPoint(0).Z);
                                XYZ p_1 = new XYZ(c.GetEndPoint(1).X, c.GetEndPoint(1).Y, curve.GetEndPoint(1).Z);
                                Line l = Line.CreateBound(p_0, p_1);
                                g.SetCurveInView(extent_mode, view, l);
                            }
                            count_2D++;
                        }
                        if(condition_side_1)
                        {
                            g.ShowBubbleInView(DatumEnds.End0, view);
                        }
                        if(!condition_side_1)
                        {
                            g.HideBubbleInView(DatumEnds.End0, view);
                        }
                        if (condition_side_2)
                        {
                            g.ShowBubbleInView(DatumEnds.End1, view);
                        }
                        if (!condition_side_2)
                        {
                            g.HideBubbleInView(DatumEnds.End1, view);
                        }
                        count++;
                    }
                    trans.Commit();

                    if (count == 0)
                    {
                        TaskDialog.Show("Grids Align", "No grids aligned");
                    }
                    else
                    {
                        TaskDialog.Show("Grids Align", string.Format("{0} grids switched to 2D and aligned {1} grids changed bubbles", count_2D.ToString(), count.ToString()));
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
    }
}
