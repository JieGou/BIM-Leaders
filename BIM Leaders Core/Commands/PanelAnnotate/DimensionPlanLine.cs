using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionPlanLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Models
            DimensionPlanLineM formM = new DimensionPlanLineM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectLineM formSelectionM = new SelectLineM(commandData);

            // ViewModel
            DimensionPlanLineVM formVM = new DimensionPlanLineVM(formM, formSelectionM);

            // View
            using (DimensionPlanLineForm form = new DimensionPlanLineForm() { DataContext = formVM})
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(form);
                helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                form.DataContext = formVM;

                if (form.ShowDialog() == false)
                    return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionPlanLine).Namespace + "." + nameof(DimensionPlanLine);
        }
    }
}
