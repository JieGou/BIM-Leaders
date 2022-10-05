using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class LevelsAlign : IExternalCommand
    {
        private static int _countLevelsAligned;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // Get user provided information from window
                LevelsAlignForm form = new LevelsAlignForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Collector for data provided in window
                LevelsAlignData data = form.DataContext as LevelsAlignData;

                // Getting input from user
                bool inputSide1 = data.ResultSide1;
                bool inputSide2 = data.ResultSide2;
                bool inputSwitch2D = data.ResultSwitch2D;
                bool inputSwitch3D = data.ResultSwitch3D;

                // Edit extents
                using (Transaction trans = new Transaction(doc, "Align Levels"))
                {
                    trans.Start();

                    DatumPlaneUtils.SetDatumPlanes(doc, typeof(Level), inputSwitch2D, inputSwitch3D, inputSide1, inputSide2, ref _countLevelsAligned);

                    trans.Commit();
                }
                ShowResult(inputSwitch2D, inputSwitch3D);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static void ShowResult(bool inputSwitch2D, bool inputSwitch3D)
        {
            // Show result
            string text = "No levels aligned.";

            if (inputSwitch2D)
                text = $"{_countLevelsAligned} levels switched to 2D and aligned.";
            else if (inputSwitch3D)
                text = $"{_countLevelsAligned} levels switched to 3D and aligned.";

            text += $"{Environment.NewLine}{_countLevelsAligned} levels changed bubbles";

            TaskDialog.Show("Levels Align", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
        }
    }
}
