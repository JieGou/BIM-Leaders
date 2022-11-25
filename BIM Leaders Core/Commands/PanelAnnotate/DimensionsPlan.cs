using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
	[Transaction(TransactionMode.Manual)]
    public class DimensionsPlan : BaseCommand
    {
        public DimensionsPlan()
        {
            _transactionName = "Dimension Plan Walls";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (ShowDialogAboutPlanRegions(commandData) == TaskDialogResult.No)
                return Result.Cancelled;

            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        /// <summary>
        /// Inform user that plan regions are on the view and may cause errors.
        /// </summary>
        /// <returns>TaskDialogResult.No if user cancelled the command.</returns>
        private TaskDialogResult ShowDialogAboutPlanRegions(ExternalCommandData commandData)
		{
            TaskDialogResult taskDialogResult = TaskDialogResult.None;

            if (CheckViewHasPlanRegions(commandData))
            {
                TaskDialog dialog = new TaskDialog(_transactionName)
                {
                    MainContent = "Plan regions are on the current view. This can cause error \"One or more dimension references are or have become invalid.\" Continue?",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                    AllowCancellation = false
                };

                taskDialogResult = dialog.Show();
            }
			return taskDialogResult;
        }

		/// <summary>
		/// Check if the current view contains plan regions.
		/// </summary>
		/// <param name="doc"></param>
		/// <returns>True if view contains plan regions, othervise false.</returns>
		private static bool CheckViewHasPlanRegions(ExternalCommandData commandData)
        {
			bool planContainsRegions = false;

            Document doc = commandData.Application.ActiveUIDocument.Document;
			IList<Element> planRegions = new FilteredElementCollector(doc, doc.ActiveView.Id)
				.OfCategory(BuiltInCategory.OST_PlanRegion)
				.ToElements();

			if (planRegions.Count > 0)
				planContainsRegions = true;

			return planContainsRegions;
		}

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionsPlanM formM = new DimensionsPlanM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionsPlanVM formVM = new DimensionsPlanVM(formM);

            // View
            DimensionsPlanForm form = new DimensionsPlanForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DimensionsPlan).Namespace + "." + nameof(DimensionsPlan);
    }
}