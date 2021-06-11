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
    public class DWG_Name_Delete : IExternalCommand
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
                DWG_Name_Delete_Data data = new DWG_Name_Delete_Data(uidoc);

                DWG_Name_Delete_Form form = new DWG_Name_Delete_Form(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as DWG_Name_Delete_Data;

                string name = doc.GetElement(data.dwg_list_sel).Category.Name;
                // Get all Imports with name same as input from a form
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<ImportInstance> dwg_types_all = collector.OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;
                List<ElementId> delete = new List<ElementId>();
                foreach (ImportInstance i in dwg_types_all)
                {
                    string i_name = i.Category.Name;
                    if (i_name == name)
                        delete.Add(i.Id);
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
                    {
                        TaskDialog.Show("DWG Deleted", "No DWG deleted");
                    }
                    else
                    {
                        TaskDialog.Show("DWG Deleted", string.Format("{0} DWG named {1} deleted", count.ToString(), name));
                    }
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
            return typeof(DWG_Name_Delete).Namespace + "." + nameof(DWG_Name_Delete);
        }
    }
}
