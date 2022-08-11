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
    [TransactionAttribute(TransactionMode.Manual)]
    public class StairsStepsEnumerate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document.
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int countGrouped = 0;
            int countUnpinned = 0;
            int countRuns = 0;
            int countTreads = 0;

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
                using (Transaction trans = new Transaction(doc, "Enumerate stairs"))
                {
                    trans.Start();

                    IOrderedEnumerable<Stairs> stairsSorted = GetStairs(doc, ref countGrouped, ref countUnpinned);
                    ChangeTreadNumbers(stairsSorted, inputStartNumber, ref countTreads);
                    CreateNumbers(doc, inputRightSide, ref countRuns);

                    trans.Commit();
                }
                ShowResult(countGrouped, countUnpinned, countRuns, countTreads);

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
        private static IOrderedEnumerable<Stairs> GetStairs(Document doc, ref int countGrouped, ref int countUnpinned)
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
                    countGrouped++;
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
                            countUnpinned++;
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
        private static void ChangeTreadNumbers(IOrderedEnumerable<Stairs> stairsSorted, double inputStartNumber, ref int countTreads)
        {
            foreach (Stairs stair in stairsSorted)
            {
                Parameter parameter = stair.get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX);
                parameter.Set(inputStartNumber);
                inputStartNumber += stair.ActualRisersNumber;

                countTreads += stair.ActualRisersNumber;
            }
        }

        /// <summary>
        /// Creating thread numbers on the view
        /// </summary>
        /// <param name="countRuns">Count of created numberings.</param>
        private static void CreateNumbers(Document doc, bool inputRightSide, ref int countRuns)
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
                    countRuns++;
                }
                catch { countRuns++; }
            }
        }

        private static void ShowResult(int countGrouped, int countUnpinned, int countRuns, int countTreads)
        {
            // Show result
            string text = "";
            if (countRuns == 0)
                text = "No annotations created";
            else
            {
                if (countGrouped > 0)
                    text += $"{countGrouped} stairs are in groups! Exclude them from groups!{Environment.NewLine}";
                text += $"{countRuns} runs with {countTreads} threads was numbered.";
                if (countUnpinned > 0)
                    text += $"{Environment.NewLine}{countUnpinned} stairs were unpinned!";
            }

            TaskDialog.Show("Section Annotations", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
        }
    }
}