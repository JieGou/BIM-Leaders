using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class DWG_View_Found : IExternalCommand
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
                IEnumerable<ImportInstance> imports = collector.OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;

                List<string> imports_names = new List<string>
                {
                    "File"
                };
                List<string> views = new List<string>
                {
                    "View"
                };
                List<string> ids = new List<string>
                {
                    "Id"
                };
                List<string> islink = new List<string>
                {
                    "Import Type"
                };

                foreach (ImportInstance i in imports)
                {
                    try
                    {
                        imports_names.Add(i.Category.Name);
                    }
                    catch (Exception empty_name)
                    {
                        imports_names.Add("");
                    }
                    
                    // Checking if 2D or 3D
                    if(i.ViewSpecific)
                    {
                        views.Add(doc.GetElement(i.OwnerViewId).Name);
                    }
                    else
                    {
                        views.Add("Not a view specific import");
                    }
                    // Checking if link or import
                    if (i.IsLinked)
                    {
                        islink.Add("Link");
                    }
                    else
                    {
                        islink.Add("Import");
                    }
                    ids.Add(i.Id.ToString());
                }

                // Export to Excel
                // ...

                // Show result
                if(imports.Count() > 0)
                {
                    TaskDialog.Show("Imports", string.Format("[{0}] on view [{1}] as {2}", imports_names[1], views[1], islink[1]));
                }
                else
                {
                    TaskDialog.Show("Imports", string.Format("No importts in the file"));
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
