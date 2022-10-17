using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using BIM_Leaders_Logic;
using System.Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloors : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (CheckIfSectionIsSplit(commandData.Application.ActiveUIDocument.Document))
                TaskDialog.Show("Annotate Section", "Current view is a split section. This may cause issues when finding geometry intersections.");

            DimensionSectionFloorsM formM = new DimensionSectionFloorsM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);

            formM.ExternalEvent = externalEvent;

            SelectLineM formSelectionM = new SelectLineM(commandData);
            DimensionSectionFloorsVM formVM = new DimensionSectionFloorsVM(formM, formSelectionM);
            DimensionSectionFloorsForm form = new DimensionSectionFloorsForm() { DataContext = formVM };

            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;
            
            return Result.Succeeded;
        }

        private static bool CheckIfSectionIsSplit(Document document)
        {
            bool result = false;
#if !VERSION2020
            ViewSection view = document.ActiveView as ViewSection;
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
