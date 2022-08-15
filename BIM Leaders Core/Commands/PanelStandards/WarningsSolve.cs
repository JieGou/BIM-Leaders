using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class WarningsSolve : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int countWarningsJoin = 0;
            int countWarningsWallsAttached = 0;
            int countWarningsRoomNotEnclosed = 0;

            try
            {
                WarningsSolveForm form = new WarningsSolveForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                WarningsSolveData data = form.DataContext as WarningsSolveData;

                // Getting input data from user
                bool fixWarningsJoin = data.ResultFixWarningsJoin;
                bool fixWarningsWallsAttached = data.ResultFixWarningsWallsAttached;
                bool fixWarningsRoomNotEnclosed = data.ResultFixWarningsRoomNotEnclosed;


                IEnumerable<FailureMessage> warnings = doc.GetWarnings();
                IEnumerable<FailureMessage> warningsJoin = warnings
                    .Where(x => x.GetDescriptionText() == "Highlighted elements are joined but do not intersect.");
                IEnumerable<FailureMessage> warningsWallsAttached = warnings
                    .Where(x => x.GetDescriptionText() == "Highlighted walls are attached to, but miss, the highlighted targets.");
                IEnumerable<FailureMessage> warningsRoomNotEnclosed = warnings
                    .Where(x => x.GetDescriptionText() == "Room is not in a properly enclosed region");

                using (Transaction trans = new Transaction(doc, "Solve Warnings"))
                {
                    trans.Start();

                    if (fixWarningsJoin)
                        SolveJoin(doc, warningsJoin, ref countWarningsJoin);
                    if (fixWarningsWallsAttached)
                        SolveWallsAttached(doc, warningsWallsAttached, ref countWarningsWallsAttached);
                    if (fixWarningsRoomNotEnclosed)
                        SolveRoomNotEnclosed(doc, warningsRoomNotEnclosed, ref countWarningsRoomNotEnclosed);
                    
                    trans.Commit();
                }
                ShowResult(countWarningsJoin, countWarningsWallsAttached, countWarningsRoomNotEnclosed);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Unjoin elements that have a warning about joining.
        /// </summary>
        private static void SolveJoin(Document doc, IEnumerable<FailureMessage> warnings, ref int count)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = doc.GetElement(ids[0]);
                Element element1 = doc.GetElement(ids[1]);

                try
                {
                    JoinGeometryUtils.UnjoinGeometry(doc, element0, element1);
                    count++;
                }
                catch { }
            }
        }

        /// <summary>
        /// Detach walls that have a warning about attachment.
        /// </summary>
        private static void SolveWallsAttached(Document doc, IEnumerable<FailureMessage> warnings, ref int count)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = doc.GetElement(ids[0]);
                Element element1 = doc.GetElement(ids[1]);

                /// HERE WILL BE SOLVING IF IT APPEARS IN THE API.
            }
        }

        /// <summary>
        /// Delete rooms that are placed but not enclosed.
        /// </summary>
        private static void SolveRoomNotEnclosed(Document doc, IEnumerable<FailureMessage> warnings, ref int count)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(doc, ids).ToList();
                if (ids.Count != 1)
                    continue;

                try
                {
                    doc.Delete(ids[0]);
                    count++;
                }
                catch { }
            }
        }

        private static void ShowResult(int countWarningsJoin, int countWarningsWallsAttached, int countWarningsRoomNotEnclosed)
        {
            // Show result
            string text = "";
            if (countWarningsJoin + countWarningsWallsAttached + countWarningsRoomNotEnclosed == 0)
                text = "No warnings solved";
            else
            {
                if (countWarningsJoin > 0)
                    text += $"{countWarningsJoin} elements join warnings";
                if (countWarningsWallsAttached > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countWarningsWallsAttached} walls attached warnings";
                }
                if (countWarningsRoomNotEnclosed > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{countWarningsRoomNotEnclosed} rooms not enclosed warnings";
                }
                text += " were solved.";
            }
            
            TaskDialog.Show("Solve Warnings", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}
