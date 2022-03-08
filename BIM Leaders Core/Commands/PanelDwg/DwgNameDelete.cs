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

            try
            {
                // Collector for data provided in window
                DwgNameDeleteData data = new DwgNameDeleteData(uidoc);

                DwgNameDeleteForm form = new DwgNameDeleteForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as DwgNameDeleteData;

                string name = doc.GetElement(data.DwgListSelected).Category.Name;

                // Get all Imports with name same as input from a form
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<ImportInstance> dwgTypesAll = collector.OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;
                List<ElementId> delete = new List<ElementId>();
                foreach (ImportInstance dwgType in dwgTypesAll)
                {
                    string dwgTypeName = dwgType.Category.Name;
                    if (dwgTypeName == name)
                        delete.Add(dwgType.Id);
                }
                int count = 0;

                using (Transaction trans = new Transaction(doc, "Delete DWG by Name"))
                {
                    trans.Start();

                    foreach(ElementId i in delete)
                    {
                        doc.Delete(i);
                        count++;
                    }

                    trans.Commit();

                    if(count == 0)
                        TaskDialog.Show("DWG Deleted", "No DWG deleted");
                    else
                        TaskDialog.Show("DWG Deleted", string.Format("{0} DWG named {1} deleted", count.ToString(), name));
                }
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
