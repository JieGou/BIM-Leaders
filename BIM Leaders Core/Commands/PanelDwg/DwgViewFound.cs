using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class DwgViewFound : IExternalCommand
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
                IEnumerable<ImportInstance> imports = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;

                /*
                List<string> namesColumn = new List<string> { "File" };
                List<string> viewsColumn = new List<string> { "View" };
                List<string> idsColumn = new List<string> { "Id" };
                List<string> islinkColumn = new List<string> { "Import Type" };

                foreach (ImportInstance import in imports)
                {
                    try
                    {
                        namesColumn.Add(import.Category.Name);
                    }
                    catch (Exception e)
                    {
                        namesColumn.Add(e.Message);
                    }
                    
                    // Checking if 2D or 3D
                    string viewText = import.ViewSpecific
                        ? doc.GetElement(import.OwnerViewId).Name
                        : "Not a view specific import";
                    viewsColumn.Add(viewText);

                    idsColumn.Add(import.Id.ToString());

                    // Checking if link or import
                    string importText = import.IsLinked
                        ? "Link"
                        : "Import";
                    islinkColumn.Add(importText);
                }

                // Export to Excel
                // ...
                */

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
                foreach (ImportInstance i in imports)
                {
                    // Name
                    string iName = i.Category?.Name ?? "Error";

                    // Checking if 2D or 3D
                    string iView = (i.ViewSpecific)
                        ? doc.GetElement(i.OwnerViewId).Name
                        : "Not a view specific import";

                    // Id
                    string iId = i.Id.ToString();

                    // Checking if link or import
                    string iLink = (i.IsLinked)
                        ? "Link"
                        : "Import";

                    DataRow newRow1 = dwgDataTable.NewRow();
                    newRow1["File"] = iName;
                    newRow1["View"] = iView;
                    newRow1["Id"] = iId;
                    newRow1["Import Type"] = iLink;

                    // Add the row to the Customers table.  
                    dwgDataTable.Rows.Add(newRow1); 
                }

                // Show result
                if (imports.Count() > 0)
                {
                    //DwgViewFoundData data = new DwgViewFoundData(dwgDataSet);
                    DwgViewFoundForm form = new DwgViewFoundForm(dwgDataSet);
                    form.ShowDialog();
                }
                else
                    TaskDialog.Show("Imports", "No imports in the file.");
                
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
            return typeof(DwgViewFound).Namespace + "." + nameof(DwgViewFound);
        }
    }
}
