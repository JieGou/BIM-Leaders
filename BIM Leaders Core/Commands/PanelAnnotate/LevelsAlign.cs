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
        private static Document _doc;
        private static LevelsAlignVM _inputData;
        private static int _countLevelsAligned;

        private const string TRANSACTION_NAME = "Align Levels";

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

                    DatumPlaneUtils.SetDatumPlanes(_doc, typeof(Level), _inputData.ResultSwitch2D, _inputData.ResultSwitch3D, _inputData.ResultSide1, _inputData.ResultSide2, ref _countLevelsAligned);

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

        private static LevelsAlignVM GetUserInput()
        {
            // Get user provided information from window
            LevelsAlignForm form = new LevelsAlignForm();
            LevelsAlignVM formVM = new LevelsAlignVM();
            form.DataContext = formVM;
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Collector for data provided in window
            return form.DataContext as LevelsAlignVM;
        }

        private static void ShowResult()
        {
            // Show result
            string text = "No levels aligned.";

            if (_inputData.ResultSwitch2D)
                text = $"{_countLevelsAligned} levels switched to 2D and aligned.";
            else if (_inputData.ResultSwitch3D)
                text = $"{_countLevelsAligned} levels switched to 3D and aligned.";

            text += $"{Environment.NewLine}{_countLevelsAligned} levels changed bubbles";

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(LevelsAlign).Namespace + "." + nameof(LevelsAlign);
        }
    }
}
