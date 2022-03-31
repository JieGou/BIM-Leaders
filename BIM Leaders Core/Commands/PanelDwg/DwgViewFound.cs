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
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<ImportInstance> imports = collector.OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;

                List<string> importsNames = new List<string> { "File" };
                List<string> views = new List<string> { "View" };
                List<string> ids = new List<string> { "Id" };
                List<string> islink = new List<string> { "Import Type" };

                foreach (ImportInstance import in imports)
                {
                    try
                    {
                        importsNames.Add(import.Category.Name);
                    }
                    catch (Exception e)
                    {
                        importsNames.Add(e.Message);
                    }
                    
                    // Checking if 2D or 3D
                    if(import.ViewSpecific)
                        views.Add(doc.GetElement(import.OwnerViewId).Name);
                    else
                        views.Add("Not a view specific import");
                    // Checking if link or import
                    if (import.IsLinked)
                        islink.Add("Link");
                    else
                        islink.Add("Import");

                    ids.Add(import.Id.ToString());
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
                string iName = "";
                string iView = "Not a view specific import";
                string iId = "";
                string iLink = "Import";
                foreach (ImportInstance i in imports)
                {
                    newRow1 = dwgDataTable.NewRow();

                    // Name
                    try
                    {
                        iName = i.Category.Name;
                    }
                    catch (Exception e) { iName = e.Message; }

                    // Checking if 2D or 3D
                    if (i.ViewSpecific)
                        iView = doc.GetElement(i.OwnerViewId).Name;

                    // Id
                    iId = i.Id.ToString();

                    // Checking if link or import
                    if (i.IsLinked)
                        iLink = "Link";

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
                    DwgViewFoundData data = new DwgViewFoundData(dwgDataSet);
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
