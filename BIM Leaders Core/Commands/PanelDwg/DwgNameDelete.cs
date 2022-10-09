using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using System.Windows.Controls;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DwgNameDelete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                List<string> dwgList = GetDwgList();
                DwgNameDeleteForm form = new DwgNameDeleteForm(doc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DwgNameDeleteVM data = form.DataContext as DwgNameDeleteVM;

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

        private static SortedDictionary<string, int> GetDwgList()
        {
            // Get DWGs
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            IEnumerable<ImportInstance> dwgTypesAll = collector.OfClass(typeof(ImportInstance)).OrderBy(a => a.Name)
                .Cast<ImportInstance>(); //LINQ function;

            // Get unique imports names list
            List<ImportInstance> dwgTypes = new List<ImportInstance>();
            List<string> dwgTypesNames = new List<string>();
            foreach (ImportInstance i in dwgTypesAll)
            {
                string dwgTypeName = i.Category.Name;
                if (!dwgTypesNames.Contains(dwgTypeName))
                {
                    dwgTypes.Add(i);
                    dwgTypesNames.Add(dwgTypeName);
                }
            }

            SortedDictionary<string, ElementId> dwgTypesList = new SortedDictionary<string, ElementId>();
            foreach (ImportInstance i in dwgTypes)
            {
                dwgTypesList.Add(i.Category.Name, i.Id);
            }

            return dwgTypesList;
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
