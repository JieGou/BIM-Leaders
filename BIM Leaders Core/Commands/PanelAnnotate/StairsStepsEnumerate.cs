using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerate : IExternalCommand
    {
        private static int _countStairsGrouped;
        private static int _countStairsUnpinned;
        private static int _countRisersNumbers;
        private static int _countRunsNumbersPlaced;

        private const string TRANSACTION_NAME = "Enumerate stairs";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document.
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // Collector for data provided in window.
                StairsStepsEnumerateForm form = new StairsStepsEnumerateForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window.
                StairsStepsEnumerateData data = form.DataContext as StairsStepsEnumerateData;

                // Getting input from user.
                bool inputRightSide = data.ResultSideRight;
                int inputStartNumber = data.ResultNumber;
                
                // Create annotations.
                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    IOrderedEnumerable<Stairs> stairsSorted = GetStairs(doc);
                    ChangeTreadNumbers(stairsSorted, inputStartNumber);
                    CreateNumbers(doc, inputRightSide);

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

        /// <summary>
        /// Get all unpinned and ungrouped stairs on the current view.
        /// </summary>
        /// <param name="doc">Current document.</param>
        /// <param name="countGrouped">Count of stairs that are in the groups and may make problems with numeration.</param>
        /// <param name="countUnpinned">Count of stairs that were unpinned from multistory stairs.</param>
        /// <returns>IOrderedEnumerable of stairs from bottom to top.</returns>
        private static IOrderedEnumerable<Stairs> GetStairs(Document doc)
        {
            IOrderedEnumerable<Stairs> stairsSorted;

            // Get all stairs in the view.
            // If multistairs, unpinned or unique (different hight) stairs will separate Stairs instance.
            IEnumerable<Stairs> stairsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
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
                Element mainStair = doc.GetElement(mainStairId);
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
                    MultistoryStairs multistoryStairs = doc.GetElement(stair.MultistoryStairsId) as MultistoryStairs;
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
        private static void ChangeTreadNumbers(IOrderedEnumerable<Stairs> stairsSorted, double inputStartNumber)
        {
            foreach (Stairs stair in stairsSorted)
            {
                Parameter parameter = stair.get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX);
                parameter.Set(inputStartNumber);
                inputStartNumber += stair.ActualRisersNumber;

                _countRisersNumbers += stair.ActualRisersNumber;
            }
        }

        /// <summary>
        /// Creating thread numbers on the view
        /// </summary>
        /// <param name="countRuns">Count of created numberings.</param>
        private static void CreateNumbers(Document doc, bool inputRightSide)
        {
            // Get all runs in the view
            IEnumerable<StairsRun> runs = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(StairsRun))
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<StairsRun>();

            foreach (StairsRun run in runs)
            {
                Reference refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.LeftQuarter);

                if (inputRightSide)
                    refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.RightQuarter);

                LinkElementId runId = new LinkElementId(run.Id);

                try
                {
                    NumberSystem.Create(doc, doc.ActiveView.Id, runId, refer);
                    _countRunsNumbersPlaced++;
                }
                catch { _countRunsNumbersPlaced++; }
            }
        }

        private static void ShowResult()
        {
            // Show result
            string text = "";
            if (_countRunsNumbersPlaced == 0)
                text = "No annotations created";
            else
            {
                if (_countStairsGrouped > 0)
                    text += $"{_countStairsGrouped} stairs are in groups! Exclude them from groups!{Environment.NewLine}";
                text += $"{_countRunsNumbersPlaced} runs with {_countRisersNumbers} threads was numbered.";
                if (_countStairsUnpinned > 0)
                    text += $"{Environment.NewLine}{_countStairsUnpinned} stairs were unpinned!";
            }

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
        }
    }
}