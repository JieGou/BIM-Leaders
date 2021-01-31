using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
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
                // Get Imports
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<ImportInstance> imports_all = collector.OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;

                // Get unique imports names list
                List<ImportInstance> imports = new List<ImportInstance>();
                List<string> imports_categories = new List<string>();
                foreach(ImportInstance i in imports_all)
                {
                    string instance_name = i.Category.Name;
                    if(!imports_categories.Contains(instance_name))
                    {
                        imports.Add(i);
                        imports_categories.Add(instance_name);
                    }
                }

                // Getting delete list from user selected item
                List<ElementId> delete = new List<ElementId>();
                using (DWG_Name_Delete_Form form = new DWG_Name_Delete_Form(imports))
                {
                    form.ShowDialog();

                    string import_name = "";
                    if(form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        import_name = form.Result();
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }

                    foreach (ImportInstance i in imports_all)
                    {
                        string i_name = i.Category.Name;
                        if(import_name.Length > 0)
                        {
                            if (i_name == import_name)
                            {
                                delete.Add(i.Id);
                            }
                        }
                    }
                }
                int count = 0;
                string name = "";
                using (Transaction trans = new Transaction(doc, "Delete DWG by Name"))
                {
                    trans.Start();

                    foreach(ElementId i in delete)
                    {
                        name = doc.GetElement(i).Category.Name;
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
    }
}
