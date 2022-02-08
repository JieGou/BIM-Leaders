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
    public class Levels_Align : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get user provided information from window
            Levels_Align_Form form = new Levels_Align_Form();
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            // Collector for data provided in window
            Levels_Align_Data data = form.DataContext as Levels_Align_Data;

            // Getting input from user
            bool inputSwitch = data.result_switch;
            bool inputSide1 = data.result_side_1;
            bool inputSide2 = data.result_side_2;

            int count2D = 0;
            int count = 0;

            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                IEnumerable<Level> levels = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Levels).ToElements().Cast<Level>();

                DatumExtentType extentMode = DatumExtentType.ViewSpecific;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Levels"))
                {
                    trans.Start();

                    Curve curve = levels.First().GetCurvesInView(extentMode, view)[0];

                    foreach (Level level in levels)
                    {
                        if (inputSwitch)
                        {
                            level.SetDatumExtentType(DatumEnds.End0, view, extentMode);
                            level.SetDatumExtentType(DatumEnds.End1, view, extentMode);

                            if (view.ViewType == ViewType.Elevation | view.ViewType == ViewType.Section)
                            {
                                Curve levelCurve = level.GetCurvesInView(extentMode, view)[0];
                                XYZ point0 = new XYZ(curve.GetEndPoint(0).X, curve.GetEndPoint(0).Y, levelCurve.GetEndPoint(0).Z);
                                XYZ point1 = new XYZ(curve.GetEndPoint(1).X, curve.GetEndPoint(1).Y, levelCurve.GetEndPoint(1).Z);
                                Line line = Line.CreateBound(point0, point1);
                                level.SetCurveInView(extentMode, view, line);
                            }
                            count2D++;
                        }
                        if (inputSide1)
                            level.ShowBubbleInView(DatumEnds.End0, view);
                        if (!inputSide1)
                            level.HideBubbleInView(DatumEnds.End0, view);
                        if (inputSide2)
                            level.ShowBubbleInView(DatumEnds.End1, view);
                        if (!inputSide2)
                            level.HideBubbleInView(DatumEnds.End1, view);
                        count++;
                    }
                    trans.Commit();

                    if (count == 0)
                        TaskDialog.Show("Levels Align", "No levels aligned");
                    else
                    {
                        TaskDialog.Show("Levels Align", string.Format("{0} levels switched to 2D and aligned." + Environment.NewLine + 
                                                                      "{1} levels changed tags",
                                                                      count2D.ToString(), count.ToString()));
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
