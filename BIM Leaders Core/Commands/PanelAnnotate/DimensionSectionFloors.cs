using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloors : IExternalCommand
    {
        private static Document _doc;

        private const string TRANSACTION_NAME = "Annotate Section";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            if (CheckIfSectionIsSplit())
                TaskDialog.Show(TRANSACTION_NAME, "Current view is a split section. This may cause issues when finding geometry intersections.");

            // Models
            DimensionSectionFloorsM formM = new DimensionSectionFloorsM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionSectionFloorsVM formVM = new DimensionSectionFloorsVM(formM, formSelectionM);

            // View
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm(formVM) { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        private static bool CheckIfSectionIsSplit()
        {
            bool result = false;
#if !VERSION2020
            ViewSection view = _doc.ActiveView as ViewSection;
            if (view.IsSplitSection())
                return true;
#endif
            return result;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionSectionFloors).Namespace + "." + nameof(DimensionSectionFloors);
        }
    }
}
