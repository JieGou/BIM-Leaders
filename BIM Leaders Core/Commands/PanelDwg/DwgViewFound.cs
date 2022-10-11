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
    [Transaction(TransactionMode.ReadOnly)]
    public class DwgViewFound : IExternalCommand
    {
        private static Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // Get Imports
                IEnumerable<ImportInstance> imports = new FilteredElementCollector(_doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>(); //LINQ function;

                if (imports.Count() == 0)
                    TaskDialog.Show("Imports", "No imports in the file.");
                else
                {
                    DataSet dwgDataSet = CreateDwgDataSet(imports);
                    ShowResult(dwgDataSet);
                    // Export to Excel
                    // ...
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Create DataSet table for report window.
        /// </summary>
        /// <returns>DataSet object contains all data.</returns>
        private static DataSet CreateDwgDataSet(IEnumerable<ImportInstance> imports)
        {
            // Create a DataSet
            DataSet dwgDataSet = new DataSet("reportDataSet");

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
                DataRow newRow = dwgDataTable.NewRow();

                // Name
                string iName = i.Category?.Name ?? "Error";

                // Checking if 2D or 3D
                string iView = (i.ViewSpecific)
                    ? _doc.GetElement(i.OwnerViewId).Name
                    : "Not a view specific import";

                // Id
                string iId = i.Id.ToString();

                // Checking if link or import
                string iLink = (i.IsLinked)
                    ? "Link"
                    : "Import";

                newRow["File"] = iName;
                newRow["View"] = iView;
                newRow["Id"] = iId;
                newRow["Import Type"] = iLink;

                dwgDataTable.Rows.Add(newRow);
            }

            return dwgDataSet;
        }

        private static void ShowResult(DataSet dwgDataSet)
        {
            // Show result
            DwgViewFoundForm form = new DwgViewFoundForm();
            DwgViewFoundVM formVM = new DwgViewFoundVM(dwgDataSet);
            form.DataContext = formVM;

            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgViewFound).Namespace + "." + nameof(DwgViewFound);
        }
    }
}
