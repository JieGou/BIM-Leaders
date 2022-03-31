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
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Document doc = uidoc.Document;

            int count = 0;
            int countGrouped = 0;
            int countUnpinned = 0;

            try
            {
                // Collector for data provided in window
                StairsStepsEnumerateForm form = new StairsStepsEnumerateForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                StairsStepsEnumerateData data = form.DataContext as StairsStepsEnumerateData;

                // Getting input from user
                bool inputRightSide = data.ResultSideRight;
                double inputStartNumber = double.Parse(data.ResultNumber);

                // Get all multistory stairs in the view
                IEnumerable<MultistoryStairs> stairsMultiAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(MultistoryStairs))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<MultistoryStairs>();

                // Get all stairs in the view
                IEnumerable<Stairs> stairsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                    .OfClass(typeof(Stairs))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Stairs>();

                // Filtering for multistairs that are in groups
                List<MultistoryStairs> stairsMulti = new List<MultistoryStairs>();
                foreach (MultistoryStairs stairMulti in stairsMultiAll)
                {
                    if (stairMulti.GroupId == ElementId.InvalidElementId)
                        stairsMulti.Add(stairMulti);
                    else
                        countGrouped++;
                }

                // Filtering for stairs that are in groups
                List<Stairs> stairs = new List<Stairs>();
                foreach (Stairs stair in stairsAll)
                {
                    if (stair.GroupId == ElementId.InvalidElementId)
                        stairs.Add(stair);
                    else
                        countGrouped++;
                }

                // Changing stairs order in a list according to base height
                IOrderedEnumerable<Stairs> stairsSorted = stairs.OrderBy(x => x.BaseElevation);

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Enumerate stairs"))
                {
                    trans.Start();

                    countUnpinned = UnpinMultistoryStairs(doc, stairsMulti);
                    ChangeTreadNumbers(stairsSorted, inputStartNumber);
                    count = CreateNumbers(doc, inputRightSide);

                    trans.Commit();
                }

                // Show result
                string text = "";
                if (count == 0)
                    text = "No annotations created";
                else
                {
                    if (countGrouped > 0)
                        text += $"{countGrouped} stairs are in groups! Exclude them from groups!{Environment.NewLine}";
                    text += $"{count} runs with {inputStartNumber - 1} threads was numbered.";
                    if (countUnpinned > 0)
                        text += $"{Environment.NewLine}{countUnpinned} stairs were unpinned!";
                    /*
                    if(pinned > 0)
                    {
                        text += pinned.ToString();
                        text += " stairs are pinned!";
                    }
                    */
                }
                TaskDialog.Show("Section Annotations", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Unpin multistory stairs. Only unpinned stairs can have different start number on each floor.
        /// </summary>
        /// <returns>Count of unpinned stairs.</returns>
        private static int UnpinMultistoryStairs(Document doc, List<MultistoryStairs> stairsMulti)
        {
            int count = 0;

            // Selecting all levels in the view
            IEnumerable<ElementId> levelsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .ToElementIds();

            // Unpinning groups (stairs) in multistairs
            foreach (MultistoryStairs stairMulti in stairsMulti)
                foreach (ElementId level in levelsAll)
                {
                    try
                    {
                        stairMulti.Unpin(level);
                        count++;
                    }
                    catch { }
                }
            return count;
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
            }
        }

        /// <summary>
        /// Creating thread numbers on the view
        /// </summary>
        /// <returns>Count of created numberings.</returns>
        private static int CreateNumbers(Document doc, bool inputRightSide)
        {
            int count = 0;

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
                    count++;
                }
                catch { count++; }
            }
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
        }
    }
}