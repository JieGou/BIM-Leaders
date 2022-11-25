using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerateM : BaseModel
    {
        private int _countStairsGrouped;
        private int _countStairsUnpinned;
        private int _countRisersNumbers;
        private int _countRunsNumbersPlaced;

        #region PROPERTIES

        private int _startNumber;
        public int StartNumber
        {
            get { return _startNumber; }
            set
            {
                _startNumber = value;
                OnPropertyChanged(nameof(StartNumber));
            }
        }

        private bool _sideRight;
        public bool SideRight
        {
            get { return _sideRight; }
            set
            {
                _sideRight = value;
                OnPropertyChanged(nameof(SideRight));
            }
        }

        #endregion

        public StairsStepsEnumerateM(
            ExternalCommandData commandData,
            string transactionName,
            Action<RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                IOrderedEnumerable<Stairs> stairsSorted = GetStairs();
                ChangeTreadNumbers(stairsSorted);
                CreateNumbers();

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        /// <summary>
        /// Get all unpinned and ungrouped stairs on the current view.
        /// </summary>
        /// <returns>IOrderedEnumerable of stairs from bottom to top.</returns>
        private IOrderedEnumerable<Stairs> GetStairs()
        {
            IOrderedEnumerable<Stairs> stairsSorted;

            // Get all stairs in the view.
            // If multistairs, unpinned or unique (different hight) stairs will separate Stairs instance.
            IEnumerable<Stairs> stairsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Stairs))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Stairs>();


            List<Stairs> stairs = new List<Stairs>();
            foreach (Stairs stair in stairsAll)
            {
                // Checking for stairs in groups.
                // Multistory stairs can be added in group only as a whole, so extract MultistoryStairs from Stairs.
                ElementId mainStairId = stair.MultistoryStairsId ?? stair.Id;
                Element mainStair = _doc.GetElement(mainStairId);
                if (mainStair.GroupId != ElementId.InvalidElementId)
                {
                    _countStairsGrouped++;
                    continue;
                }

                // Collect unpinned or unique (can be numbered also) stairs from multistory stairs.
                // Else unpin stairs in multistory stairs. Store them in "subelements" list.
                IList<Subelement> subelements = stair.GetSubelements();
                if (subelements.Count == 0)
                    stairs.Add(stair);
                else
                {
                    MultistoryStairs multistoryStairs = _doc.GetElement(stair.MultistoryStairsId) as MultistoryStairs;
                    ISet<ElementId> levelIds = multistoryStairs.GetStairsPlacementLevels(stair);
                    foreach (ElementId levelId in levelIds)
                    {
                        try
                        {
                            multistoryStairs.Unpin(levelId);
                            _countStairsUnpinned++;
                            stairs.Add(multistoryStairs.GetStairsOnLevel(levelId));
                        }
                        catch { }
                    }
                }
            }

            // Changing stairs order in a list according to base height.
            stairsSorted = stairs.OrderBy(x => x.BaseElevation);

            return stairsSorted;
        }

        /// <summary>
        /// Change tread numbers of all given stairs, beginning from the given number.
        /// </summary>
        private void ChangeTreadNumbers(IOrderedEnumerable<Stairs> stairsSorted)
        {
            foreach (Stairs stair in stairsSorted)
            {
                Parameter parameter = stair.get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX);
                parameter.Set(StartNumber);
                StartNumber += stair.ActualRisersNumber;

                _countRisersNumbers += stair.ActualRisersNumber;
            }
        }

        /// <summary>
        /// Creating thread numbers on the view
        /// </summary>
        private void CreateNumbers()
        {
            // Get all runs in the view
            IEnumerable<StairsRun> runs = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(StairsRun))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<StairsRun>();

            foreach (StairsRun run in runs)
            {
                Reference refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.LeftQuarter);

                if (SideRight)
                    refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.RightQuarter);

                LinkElementId runId = new LinkElementId(run.Id);

                try
                {
                    NumberSystem.Create(_doc, _doc.ActiveView.Id, runId, refer);
                    _countRunsNumbersPlaced++;
                }
                catch { _countRunsNumbersPlaced++; }
            }
        }

        private protected override string GetRunResult()
        {
            string text = "";

            if (_countRunsNumbersPlaced == 0)
                text = "No annotations created.";
            else
            {
                if (_countStairsGrouped > 0)
                    text += $"{_countStairsGrouped} stairs are in groups! Exclude them from groups!{Environment.NewLine}";
                text += $"{_countRunsNumbersPlaced} runs with {_countRisersNumbers} threads was numbered.";
                if (_countStairsUnpinned > 0)
                    text += $"{Environment.NewLine}{_countStairsUnpinned} stairs were unpinned!";
            }

            return text;
        }

        #endregion
    }
}