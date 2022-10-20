using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class GridsAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Model
            GridsAlignM formM = new GridsAlignM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            GridsAlignVM formVM = new GridsAlignVM(formM);

            // View
            GridsAlignForm form = new GridsAlignForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
        }
    }
}
