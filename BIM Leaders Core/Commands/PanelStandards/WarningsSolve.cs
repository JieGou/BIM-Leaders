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
        private static Document _doc;
        private static int _countWarningsJoin;
        private static int _countWarningsWallsAttached;
        private static int _countWarningsRoomNotEnclosed;
        private static WarningsSolveVM _inputData;

        private const string TRANSACTION_NAME = "Solve Warnings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    SolveAll();

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

        private static WarningsSolveVM GetUserInput()
        {
            WarningsSolveForm form = new WarningsSolveForm();
            WarningsSolveVM formVM = new WarningsSolveVM();
            form.DataContext = formVM;
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as WarningsSolveVM;
        }

        private static void SolveAll()
        {
            IEnumerable<FailureMessage> warnings = _doc.GetWarnings();
            IEnumerable<FailureMessage> warningsJoin = warnings
                    .Where(x => x.GetDescriptionText() == "Highlighted elements are joined but do not intersect.");
            IEnumerable<FailureMessage> warningsWallsAttached = warnings
                .Where(x => x.GetDescriptionText() == "Highlighted walls are attached to, but miss, the highlighted targets.");
            IEnumerable<FailureMessage> warningsRoomNotEnclosed = warnings
                .Where(x => x.GetDescriptionText() == "Room is not in a properly enclosed region");

            if (_inputData.ResultFixWarningsJoin)
                SolveJoin(warningsJoin);
            if (_inputData.ResultFixWarningsWallsAttached)
                SolveWallsAttached(warningsWallsAttached);
            if (_inputData.ResultFixWarningsRoomNotEnclosed)
                SolveRoomNotEnclosed(warningsRoomNotEnclosed);
        }

        /// <summary>
        /// Unjoin elements that have a warning about joining.
        /// </summary>
        private static void SolveJoin(IEnumerable<FailureMessage> warnings)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (_doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(_doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = _doc.GetElement(ids[0]);
                Element element1 = _doc.GetElement(ids[1]);

                try
                {
                    JoinGeometryUtils.UnjoinGeometry(_doc, element0, element1);
                    _countWarningsJoin++;
                }
                catch { }
            }
        }

        /// <summary>
        /// Detach walls that have a warning about attachment.
        /// </summary>
        private static void SolveWallsAttached(IEnumerable<FailureMessage> warnings)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (_doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(_doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = _doc.GetElement(ids[0]);
                Element element1 = _doc.GetElement(ids[1]);

                /// HERE WILL BE SOLVING IF IT APPEARS IN THE API.
            }
        }

        /// <summary>
        /// Delete rooms that are placed but not enclosed.
        /// </summary>
        private static void SolveRoomNotEnclosed(IEnumerable<FailureMessage> warnings)
        {
            foreach (FailureMessage warning in warnings)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (_doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(_doc, ids).ToList();
                if (ids.Count != 1)
                    continue;

                try
                {
                    _doc.Delete(ids[0]);
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