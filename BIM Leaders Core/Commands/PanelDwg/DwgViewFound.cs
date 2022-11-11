using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;
using System.Threading.Tasks;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.ReadOnly)]
    public class DwgViewFound : IExternalCommand
    {
        private Document _doc;
        private DataSet _dwgList;

        private const string TRANSACTION_NAME = "Imports";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            _dwgList = GetDwgList(commandData);

            if (_dwgList != null)
                Run(commandData);

            return Result.Succeeded;
        }

        private DataSet GetDwgList(ExternalCommandData commandData)
        {
            try
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                // Get Imports
                IEnumerable<ImportInstance> imports = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Cast<ImportInstance>();

                if (imports.Count() == 0)
                {
                    ShowResult("Document has no DWG.");
                    return null;
                }   
                else
                {
                    DataSet dwgDataSet = CreateDwgDataSet(imports);
                    return dwgDataSet;
                }
            }
            catch (Exception e)
            {
                ShowResult(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Create DataSet table for report window.
        /// </summary>
        /// <returns>DataSet object contains all data.</returns>
        private DataSet CreateDwgDataSet(IEnumerable<ImportInstance> imports)
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

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            DwgViewFoundM formM = new DwgViewFoundM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DwgViewFoundVM formVM = new DwgViewFoundVM(formM)
            {
                DwgList = _dwgList
            };

            // View
            DwgViewFoundForm form = new DwgViewFoundForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            if (resultText == null)
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgViewFound).Namespace + "." + nameof(DwgViewFound);
        }
    }
}