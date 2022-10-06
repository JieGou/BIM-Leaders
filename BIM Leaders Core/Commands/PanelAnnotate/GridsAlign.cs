using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class GridsAlign : IExternalCommand
    {
        private static Document _doc;
        private static GridsAlignData _inputData;
        private static int _countGridsAligned;

        private const string TRANSACTION_NAME = "Align Grids";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    DatumPlaneUtils.SetDatumPlanes(_doc, typeof(Grid), _inputData.ResultSwitch2D, _inputData.ResultSwitch3D, _inputData.ResultSide1, _inputData.ResultSide2, ref _countGridsAligned);

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
        
        private static GridsAlignData GetUserInput()
        {
            // Get user provided information from window
            GridsAlignForm form = new GridsAlignForm();
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Collector for data provided in window
            return form.DataContext as GridsAlignData;
        }

        private static void ShowResult()
        {
            // Show result
            string text = "No grids aligned.";

            if (_inputData.ResultSwitch2D)
                text = $"{_countGridsAligned} grids switched to 2D and aligned.";
            else if (_inputData.ResultSwitch3D)
                text = $"{_countGridsAligned} grids switched to 3D and aligned.";

            text += $"{Environment.NewLine}{_countGridsAligned} grids changed bubbles";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(GridsAlign).Namespace + "." + nameof(GridsAlign);
        }
    }
}
