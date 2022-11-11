using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class HelpStandards : IExternalCommand
    {
        private const string URL = @"https://bimleaders.sharepoint.com/:o:/r/sites/BIMAcademy-Archtecture/Shared%20Documents/Architecture/Standards?d=w1010ae6834644745b60c696943c0e12b&csf=1&web=1&e=6or6hJ";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                System.Diagnostics.Process.Start(URL);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(HelpStandards).Namespace + "." + nameof(HelpStandards);
        }
    }
}