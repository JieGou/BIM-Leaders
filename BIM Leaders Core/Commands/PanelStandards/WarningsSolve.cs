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
    [Transaction(TransactionMode.Manual)]
    public class WarningsSolve : IExternalCommand
    {
        private static int _countWarningsJoin;
        private static int _countWarningsWallsAttached;
        private static int _countWarningsRoomNotEnclosed;
        private static WarningsSolveData _inputData;

        private const string TRANSACTION_NAME = "Solve Warnings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    SolveAll(doc);

                    trans.Commit();
                }
                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static WarningsSolveData GetUserInput()
        {
            WarningsSolveForm form = new WarningsSolveForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as WarningsSolveData;
        }

        private static void SolveAll(Document doc)
        {
            IEnumerable<FailureMessage> warnings = doc.GetWarnings();
            IEnumerable<FailureMessage> warningsJoin = warnings
                    .Where(x => x.GetDescriptionText() == "Highlighted elements are joined but do not intersect.");
            IEnumerable<FailureMessage> warningsWallsAttached = warnings
                .Where(x => x.GetDescriptionText() == "Highlighted walls are attached to, but miss, the highlighted targets.");
            IEnumerable<FailureMessage> warningsRoomNotEnclosed = warnings
                .Where(x => x.GetDescriptionText() == "Room is not in a properly enclosed region");

            if (_inputData.ResultFixWarningsJoin)
                SolveJoin(doc, warningsJoin);
            if (_inputData.ResultFixWarningsWallsAttached)
                SolveWallsAttached(doc, warningsWallsAttached);
            if (_inputData.ResultFixWarningsRoomNotEnclosed)
                SolveRoomNotEnclosed(doc, warningsRoomNotEnclosed);
        }

        /// <summary>
        /// Unjoin elements that have a warning about joining.
        /// </summary>
        private static void SolveJoin(Document doc, IEnumerable<FailureMessage> warnings)
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
                    _countWarningsJoin++;
                }
                catch { }
            }
        }

        /// <summary>
        /// Detach walls that have a warning about attachment.
        /// </summary>
        private static void SolveWallsAttached(Document doc, IEnumerable<FailureMessage> warnings)
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
        private static void SolveRoomNotEnclosed(Document doc, IEnumerable<FailureMessage> warnings)
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
                    _countWarningsRoomNotEnclosed++;
                }
                catch { }
            }
        }

        private static void ShowResult()
        {
            // Show result
            string text = "";
            if (_countWarningsJoin + _countWarningsWallsAttached + _countWarningsRoomNotEnclosed == 0)
                text = "No warnings solved";
            else
            {
                if (_countWarningsJoin > 0)
                    text += $"{_countWarningsJoin} elements join warnings";
                if (_countWarningsWallsAttached > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countWarningsWallsAttached} walls attached warnings";
                }
                if (_countWarningsRoomNotEnclosed > 0)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{_countWarningsRoomNotEnclosed} rooms not enclosed warnings";
                }
                text += " were solved.";
            }
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}
