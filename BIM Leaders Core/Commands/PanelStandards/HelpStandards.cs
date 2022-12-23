using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

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
            _result = new RunResult() { Started = true };

            try
            {
                System.Diagnostics.Process.Start(URL);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _result.Failed = true;
                _result.Result = e.Message;
                ShowResult(_result);
                return Result.Failed;
            }
        }

        private protected override void Run(ExternalCommandData commandData) { }

        public static string GetPath() => typeof(HelpStandards).Namespace + "." + nameof(HelpStandards);
    }
}