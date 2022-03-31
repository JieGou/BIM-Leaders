using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DwgNameDelete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int count = 0;

            try
            {
                DwgNameDeleteForm form = new DwgNameDeleteForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                DwgNameDeleteData data = form.DataContext as DwgNameDeleteData;

                string name = doc.GetElement(data.DwgListSelected).Category.Name;

                // Get all Imports with name same as input from a form
                IEnumerable<ImportInstance> dwgTypesAll = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;
                List<ElementId> delete = new List<ElementId>();
                foreach (ImportInstance dwgType in dwgTypesAll)
                    if (dwgType.Category.Name == name)
                        delete.Add(dwgType.Id);

                using (Transaction trans = new Transaction(doc, "Delete DWG by Name"))
                {
                    trans.Start();

                    foreach(ElementId i in delete)
                    {
                        doc.Delete(i);
                        count++;
                    }

                    trans.Commit();
                }

                // Show result
                string text = count == 0
                    ? "No DWG deleted"
                    : $"{count} DWG named {name} deleted";
                TaskDialog.Show("DWG Deleted", text);

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
            return typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
        }
    }
}
