using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Data;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
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

                // Create a DataSet
                DataSet dwgDataSet = new DataSet("dwgDataSet");
                // Create DataTable
                DataTable dwgDataTable = new DataTable("DWG");
                // Create 4 columns, and add them to the table
                DataColumn dwgColumnFile = new DataColumn("File", typeof(string));
                DataColumn dwgColumnView = new DataColumn("View", typeof(string));
                DataColumn dwgColumnId = new DataColumn("Id", typeof(string));
                DataColumn dwgColumnImportType = new DataColumn("Import Type", typeof(string));

                dwgDataTable.Columns.Add(dwgColumnFile);
                dwgDataTable.Columns.Add(dwgColumnView);
                dwgDataTable.Columns.Add(dwgColumnId);
                dwgDataTable.Columns.Add(dwgColumnImportType);

                // Add the table to the DataSet
                dwgDataSet.Tables.Add(dwgDataTable);

                // Fill the table
                DataRow newRow1;
                string i_name = "";
                string i_view = "Not a view specific import";
                string i_id = "";
                string i_link = "Import";
                foreach (ImportInstance i in imports)
                {
                    newRow1 = dwgDataTable.NewRow();

                    // Name
                    try
                    {
                        i_name = i.Category.Name;
                    }
                    catch (Exception empty_name) { }

                    // Checking if 2D or 3D
                    if (i.ViewSpecific)
                        i_view = doc.GetElement(i.OwnerViewId).Name;

                    // Id
                    i_id = i.Id.ToString();

                    // Checking if link or import
                    if (i.IsLinked)
                        i_link = "Link";

                    newRow1["File"] = i_name;
                    newRow1["View"] = i_view;
                    newRow1["Id"] = i_id;
                    newRow1["Import Type"] = i_link;

                    // Add the row to the Customers table.  
                    dwgDataTable.Rows.Add(newRow1); 
                }


                // Show result
                if (imports.Count() > 0)
                {
                    //TaskDialog.Show("Imports", string.Format("[{0}] on view [{1}] as {2}", imports_names[1], views[1], islink[1]));

                    DWG_View_Found_Data data = new DWG_View_Found_Data(dwgDataSet);

                    DWG_View_Found_Form form = new DWG_View_Found_Form(dwgDataSet);
                    form.ShowDialog();
                }
                else
                {
                    TaskDialog.Show("Imports", string.Format("No imports in the file"));
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
            return typeof(DWG_View_Found).Namespace + "." + nameof(DWG_View_Found);
        }
    }
}
