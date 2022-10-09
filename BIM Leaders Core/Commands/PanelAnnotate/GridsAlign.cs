using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GridsAlign : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            int count = 0;

            try
            {
                // Get user provided information from window
                GridsAlignForm form = new GridsAlignForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Collector for data provided in window
                GridsAlignVM data = form.DataContext as GridsAlignVM;

                // Getting input from user
                bool inputSide1 = data.ResultSide1;
                bool inputSide2 = data.ResultSide2;
                bool inputSwitch2D = data.ResultSwitch2D;
                bool inputSwitch3D = data.ResultSwitch3D;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Grids"))
                {
                    trans.Start();

                    DatumPlaneUtils.SetDatumPlanes(doc, typeof(Grid), inputSwitch2D, inputSwitch3D, inputSide1, inputSide2, ref count);

                    trans.Commit();
                }
                ShowResult(count, inputSwitch2D, inputSwitch3D);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        
        private static void ShowResult(int count, bool inputSwitch2D, bool inputSwitch3D)
        {
            // Show result
            string text = "No grids aligned.";

            if (inputSwitch2D)
                text = $"{count} grids switched to 2D and aligned.";
            else if (inputSwitch3D)
                text = $"{count} grids switched to 3D and aligned.";

            text += $"{Environment.NewLine}{count} grids changed bubbles";
            
            TaskDialog.Show("Grids Align", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
        }
    }
}
