using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerateM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;
        private int _countStairsGrouped;
        private int _countStairsUnpinned;
        private int _countRisersNumbers;
        private int _countRunsNumbersPlaced;

        private const string TRANSACTION_NAME = "Number Steps";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

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

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            {
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        #endregion

        public StairsStepsEnumerateM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TRANSACTION_NAME;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    IOrderedEnumerable<Stairs> stairsSorted = GetStairs();
                    ChangeTreadNumbers(stairsSorted);
                    CreateNumbers();

                    trans.Commit();
                }

                GetRunResult();
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }
        }

        #endregion

        #region METHODS

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

        private void GetRunResult()
        {
            if (RunResult.Length == 0)
            {
                if (_countRunsNumbersPlaced == 0)
                    RunResult = "No annotations created.";
                else
                {
                    if (_countStairsGrouped > 0)
                        RunResult += $"{_countStairsGrouped} stairs are in groups! Exclude them from groups!{Environment.NewLine}";
                    RunResult += $"{_countRunsNumbersPlaced} runs with {_countRisersNumbers} threads was numbered.";
                    if (_countStairsUnpinned > 0)
                        RunResult += $"{Environment.NewLine}{_countStairsUnpinned} stairs were unpinned!";
                }
            }
        }

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}