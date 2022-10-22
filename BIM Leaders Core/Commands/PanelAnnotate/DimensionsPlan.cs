using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
	[Transaction(TransactionMode.Manual)]
    public class DimensionsPlan : IExternalCommand
	{
        private static Document _doc;

        private const string TRANSACTION_NAME = "Dimension Plan Walls";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            if (ShowDialogAboutPlanRegions() == TaskDialogResult.No)
                return Result.Cancelled;

            // Models
            DimensionsPlanM formM = new DimensionsPlanM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionsPlanVM formVM = new DimensionsPlanVM(formM);

            // View
            DimensionsPlanForm form = new DimensionsPlanForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        /// <summary>
        /// Inform user that plan regions are on the view and may cause errors.
        /// </summary>
        /// <returns>TaskDialogResult.No if user cancelled the command.</returns>
        private static TaskDialogResult ShowDialogAboutPlanRegions()
		{
            TaskDialogResult taskDialogResult = TaskDialogResult.None;

            if (CheckViewHasPlanRegions())
            {
                TaskDialog dialog = new TaskDialog(TRANSACTION_NAME)
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
		private static bool CheckViewHasPlanRegions()
        {
			bool planContainsRegions = false;

			IList<Element> planRegions = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
				.OfCategory(BuiltInCategory.OST_PlanRegion)
				.ToElements();

			if (planRegions.Count > 0)
				planContainsRegions = true;

			return planContainsRegions;
		}

		public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlan).Namespace + "." + nameof(DimensionsPlan);
        }
    }
}
