using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class HelpStandards : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Help";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private const string URL = @"https://bimleaders.sharepoint.com/:o:/r/sites/BIMAcademy-Archtecture/Shared%20Documents/Architecture/Standards?d=w1010ae6834644745b60c696943c0e12b&csf=1&web=1&e=6or6hJ";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _runStarted = true;

            try
            {
                System.Diagnostics.Process.Start(URL);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult();
                return Result.Failed;
            }
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ResultVM formVM = new ResultVM(TRANSACTION_NAME, _runResult);

            // View
            ResultForm form = new ResultForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(HelpStandards).Namespace + "." + nameof(HelpStandards);
        }
    }
}