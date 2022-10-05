using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DwgNameDelete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                DwgNameDeleteForm form = new DwgNameDeleteForm(doc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DwgNameDeleteData data = form.DataContext as DwgNameDeleteData;

                string name = doc.GetElement(data.DwgListSelected).Category.Name;

                // Get all Imports with name same as input from a form
                ICollection<ElementId> dwgDelete = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Category.Name == name) //LINQ function
                    .ToList()                            //LINQ function
                    .ConvertAll(x => x.Id)               //LINQ function
                    .ToList();                           //LINQ function

                using (Transaction trans = new Transaction(doc, "Delete DWG by Name"))
                {
                    trans.Start();

                    doc.Delete(dwgDelete);  

                    trans.Commit();
                }
                ShowResult(dwgDelete.Count, name);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static void ShowResult(int count, string name)
        {
            // Show result
            string text = (count == 0)
                ? "No DWG deleted"
                : $"{count} DWG named {name} deleted";
            
            TaskDialog.Show("DWG Deleted", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
        }
    }
}
