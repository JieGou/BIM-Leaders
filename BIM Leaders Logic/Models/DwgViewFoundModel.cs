using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class DwgViewFoundModel : BaseModel
    {
        #region PROPERTIES

        public string SelectedDwg { get; set; }

        #endregion

        #region METHODS

        public DataSet GetDwgTable()
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

            IEnumerable<ImportInstance> imports = new FilteredElementCollector(Doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>();

            // Fill the table
            foreach (ImportInstance i in imports)
            {
                DataRow newRow = dwgDataTable.NewRow();

                // Name
                string iName = i.Category?.Name ?? "Error";

                // Checking if 2D or 3D
                string iView = (i.ViewSpecific)
                    ? Doc.GetElement(i.OwnerViewId).Name
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

        private protected override void TryExecute()
        {
            // Get Imports
            IEnumerable<ImportInstance> imports = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>();

            int dwgId = 0;
            if (!Int32.TryParse(SelectedDwg, out dwgId))
            {
                Result.Result = "Error getting a DWG from the selected item.";
                TaskDialog.Show(TransactionName, Result.Result);
                return;
            }

            Element dwg = Doc.GetElement(new ElementId(dwgId));
            List<ElementId> selectionSet = new List<ElementId>() { new ElementId(dwgId) };

            if (dwg.ViewSpecific)
            {
                View view = Doc.GetElement(dwg.OwnerViewId) as View;
                Uidoc.ActiveView = view;
            }

            Uidoc.Selection.SetElementIds(selectionSet);
        }

        #endregion
    }
}