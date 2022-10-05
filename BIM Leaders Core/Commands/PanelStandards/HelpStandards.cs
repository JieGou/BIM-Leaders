using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class HelpStandards : IExternalCommand
    {
        private const string URL = @"https://bimleaders.sharepoint.com/sites/Standards/SitePages/Israel%20Standards.aspx";

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