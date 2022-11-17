using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class HelpStandards : BaseCommand
    {
        private const string URL = @"https://bimleaders.sharepoint.com/:o:/r/sites/BIMAcademy-Archtecture/Shared%20Documents/Architecture/Standards?d=w1010ae6834644745b60c696943c0e12b&csf=1&web=1&e=6or6hJ";

        public HelpStandards()
        {
            _transactionName = "Help";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
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

        private protected override async void Run(ExternalCommandData commandData) { return; }

        public static string GetPath()
        {
            return typeof(HelpStandards).Namespace + "." + nameof(HelpStandards);
        }
    }
}