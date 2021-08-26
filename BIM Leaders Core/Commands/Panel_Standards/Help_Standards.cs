using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Help_Standards : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // URL
            string urlToOpen = @"https://bimleaders.sharepoint.com/sites/Standards/SitePages/Israel%20Standards.aspx";

            try
            {
                System.Diagnostics.Process.Start(urlToOpen);

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
            return typeof(Help_Standards).Namespace + "." + nameof(Help_Standards);
        }
    }
}