using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionsPlanCheck : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Model
            DimensionsPlanCheckM formM = new DimensionsPlanCheckM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionsPlanCheckVM formVM = new DimensionsPlanCheckVM(formM);

            // View
            DimensionsPlanCheckForm form = new DimensionsPlanCheckForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionsPlanCheck).Namespace + "." + nameof(DimensionsPlanCheck);
        }
    }
}
