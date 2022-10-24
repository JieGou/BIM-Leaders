using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class StairsStepsEnumerate : IExternalCommand
    {
        private static Document _doc;
        private static StairsStepsEnumerateVM _inputData;
        private static int _countStairsGrouped;
        private static int _countStairsUnpinned;
        private static int _countRisersNumbers;
        private static int _countRunsNumbersPlaced;

        private const string TRANSACTION_NAME = "Enumerate stairs";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Model
            StairsStepsEnumerateM formM = new StairsStepsEnumerateM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            StairsStepsEnumerateVM formVM = new StairsStepsEnumerateVM(formM);

            // View
            StairsStepsEnumerateForm form = new StairsStepsEnumerateVM() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;


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
                ShowResult();

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
            return typeof(StairsStepsEnumerate).Namespace + "." + nameof(StairsStepsEnumerate);
        }
    }
}