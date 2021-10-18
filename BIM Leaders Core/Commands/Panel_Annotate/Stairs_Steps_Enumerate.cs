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
    public class Stairs_Steps_Enumerate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                // Collector for data provided in window
                Stairs_Steps_Enumerate_Data data = new Stairs_Steps_Enumerate_Data();

                Stairs_Steps_Enumerate_Form form = new Stairs_Steps_Enumerate_Form();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Stairs_Steps_Enumerate_Data;

                // Getting input from user
                bool inputRightSide = data.result_side_right;
                double inputStartNumber = double.Parse(data.result_number);
                int count = 0;
                int grouped = 0;
                int unpinned = 0;

                // Get Floors
                IEnumerable<MultistoryStairs> stairsMultiAll = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_MultistoryStairs)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<MultistoryStairs>();

                // Selecting all levels in the view
                IEnumerable<ElementId> levelsAll = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Selecting all runs in the view
                IEnumerable<StairsRun> runs = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StairsRuns)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsRun>();

                // Selecting all stairs in the view
                IEnumerable<Stairs> stairsAll = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Stairs)
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
                        grouped++;
                }

                // Filtering for stairs that are in groups
                List<Stairs> stairsTemp = new List<Stairs>();
                foreach (Stairs stair in stairsAll)
                {
                    if (stair.GroupId == ElementId.InvalidElementId)
                        stairsTemp.Add(stair);
                    else
                        grouped++;
                }

                // Creating list of stairs levels and filtering for Model-In-Place
                List<Stairs> stairs = new List<Stairs>();
                List<double> levels = new List<double>();
                foreach (Stairs stair in stairsTemp)
                {
                    try
                    {
                        levels.Add(stair.BaseElevation);
                        stairs.Add(stair);

                    }
                    catch { }
                }

                // Changing stairs order in a list according to base height
                IOrderedEnumerable<Stairs> stairsSorted = stairs.OrderBy(x => x.BaseElevation);

                // Create annotations
                using (Transaction trans = new Transaction(doc, "Enumerate stairs"))
                {
                    trans.Start();

                    // Unpinning groups (stairs) in multistairs
                    foreach (MultistoryStairs stairMulti in stairsMulti)
                    {
                        foreach (ElementId level in levelsAll)
                        {
                            try
                            {
                                stairMulti.Unpin(level);
                                unpinned++;
                            }
                            catch { }
                        }
                    }

                    // Changing thread numbers
                    foreach (Stairs stair in stairsSorted)
                    {
                        Parameter parameter = stair.get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX);
                        parameter.Set(inputStartNumber);
                        inputStartNumber += stair.ActualRisersNumber;
                    }

                    // Creating thread numbers on the view
                    foreach (StairsRun run in runs)
                    {
                        Reference refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.LeftQuarter);

                        if (inputRightSide)
                            refer = run.GetNumberSystemReference(StairsNumberSystemReferenceOption.RightQuarter);

                        LinkElementId runId = new LinkElementId(run.Id);

                        try
                        {
                            NumberSystem.Create(doc, view.Id, runId, refer);
                            count++;
                        }
                        catch { count++; }
                    }

                    trans.Commit();

                    string text = "";
                    if (grouped > 0)
                    {
                        text += grouped.ToString();
                        text += " stairs are in groups! Exclude them from groups!";
                    }
                    text += count.ToString();
                    text += " runs with ";
                    text += (inputStartNumber - 1).ToString();
                    text += " threads was numbered. ";
                    if (unpinned > 0)
                    {
                        text += unpinned.ToString();
                        text += " stairs was unpinned!";
                    }
                    /*
                    if(pinned > 0)
                    {
                        text += pinned.ToString();
                        text += " stairs are pinned!";
                    }
                    */

                    if (count == 0)
                        TaskDialog.Show("Section Annotations", "No annotations created");
                    else
                        TaskDialog.Show("Section Annotations", text);
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Stairs_Steps_Enumerate).Namespace + "." + nameof(Stairs_Steps_Enumerate);
        }
    }
}