using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class WarningsSolveModel : BaseModel
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

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                SolveAll();

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        private void SolveAll()
        {
            IEnumerable<FailureMessage> warnings = Doc.GetWarnings();
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
                if (Doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(Doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = Doc.GetElement(ids[0]);
                Element element1 = Doc.GetElement(ids[1]);

                try
                {
                    JoinGeometryUtils.UnjoinGeometry(Doc, element0, element1);
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
                if (Doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(Doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = Doc.GetElement(ids[0]);
                Element element1 = Doc.GetElement(ids[1]);

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
                if (Doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(Doc, ids).ToList();
                if (ids.Count != 1)
                    continue;

                try
                {
                    Doc.Delete(ids[0]);
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