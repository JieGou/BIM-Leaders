﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class LevelsAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int count2D = 0;
            int count = 0;

            try
            {
                // Get user provided information from window
                LevelsAlignForm form = new LevelsAlignForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Collector for data provided in window
                LevelsAlignData data = form.DataContext as LevelsAlignData;

                // Getting input from user
                bool inputSwitch = data.ResultSwitch;
                bool inputSide1 = data.ResultSide1;
                bool inputSide2 = data.ResultSide2;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Levels"))
                {
                    trans.Start();

                    (count2D, count) = EditExtentsLevels(doc, inputSwitch, inputSide1, inputSide2);

                    trans.Commit();
                }
                ShowResult(count, count2D);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Edit levels extents.
        /// </summary>
        /// <returns>Count of levels switched to 2D and count of levels processed.</returns>
        private static (int, int) EditExtentsLevels(Document doc, bool inputSwitch, bool inputSide1, bool inputSide2)
        {
            int count2D = 0;
            int count = 0;

            View view = doc.ActiveView;

            DatumExtentType extentMode = DatumExtentType.ViewSpecific;

            IEnumerable<Level> levels = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Levels)
                    .ToElements()
                    .Cast<Level>();

            Curve curve = levels.FirstOrDefault().GetCurvesInView(extentMode, view)[0];

            foreach (Level level in levels)
            {
                if (inputSwitch)
                {
                    level.SetDatumExtentType(DatumEnds.End0, view, extentMode);
                    level.SetDatumExtentType(DatumEnds.End1, view, extentMode);

                    if (view.ViewType == ViewType.Elevation || view.ViewType == ViewType.Section)
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
            return (count2D, count);
        }

        private static void ShowResult(int count, int count2D)
        {
            // Show result
            string text = (count == 0)
                ? "No grids aligned"
                : $"{count2D} levels switched to 2D and aligned.{Environment.NewLine}{count} levels changed tags";
            
            TaskDialog.Show("Levels Align", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
        }
    }
}
