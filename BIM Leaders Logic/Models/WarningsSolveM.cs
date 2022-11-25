using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class WarningsSolveM : BaseModel
    {
        private int _countWarningsJoin;
        private int _countWarningsWallsAttached;
        private int _countWarningsRoomNotEnclosed;

        #region PROPERTIES

        private bool _fixWarningsJoin;
        public bool FixWarningsJoin
        {
            get { return _fixWarningsJoin; }
            set
            {
                _fixWarningsJoin = value;
                OnPropertyChanged(nameof(FixWarningsJoin));
            }
        }

        private bool _fixWarningsWallsAttached;
        public bool FixWarningsWallsAttached
        {
            get { return _fixWarningsWallsAttached; }
            set
            {
                _fixWarningsWallsAttached = value;
                OnPropertyChanged(nameof(FixWarningsWallsAttached));
            }
        }

        private bool _fixWarningsRoomNotEnclosed;
        public bool FixWarningsRoomNotEnclosed
        {
            get { return _fixWarningsRoomNotEnclosed; }
            set
            {
                _fixWarningsRoomNotEnclosed = value;
                OnPropertyChanged(nameof(FixWarningsRoomNotEnclosed));
            }
        }

        #endregion

        public WarningsSolveM(
            ExternalCommandData commandData,
            string transactionName,
            Action<string, RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                SolveAll();

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        private void SolveAll()
        {
            IEnumerable<FailureMessage> warnings = _doc.GetWarnings();
            IEnumerable<FailureMessage> warningsJoin = warnings
                .Where(x => x.GetDescriptionText() == "Highlighted elements are joined but do not intersect.");
            IEnumerable<FailureMessage> warningsWallsAttached = warnings
                .Where(x => x.GetDescriptionText() == "Highlighted walls are attached to, but miss, the highlighted targets.");
            IEnumerable<FailureMessage> warningsRoomNotEnclosed = warnings
                .Where(x => x.GetDescriptionText() == "Room is not in a properly enclosed region");

            if (FixWarningsJoin)
                SolveJoin(warningsJoin);
            if (FixWarningsWallsAttached)
                SolveWallsAttached(warningsWallsAttached);
            if (FixWarningsRoomNotEnclosed)
                SolveRoomNotEnclosed(warningsRoomNotEnclosed);
        }

        /// <summary>
        /// Unjoin elements that have a warning about joining.
        /// </summary>
        private void SolveJoin(IEnumerable<FailureMessage> warnings)
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
        private void SolveWallsAttached(IEnumerable<FailureMessage> warnings)
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
        private void SolveRoomNotEnclosed(IEnumerable<FailureMessage> warnings)
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

        private protected override string GetRunResult()
        {
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

            return text;
        }

        #endregion
    }
}