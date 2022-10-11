using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using System.Windows.Controls;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DwgNameDelete : IExternalCommand
    {
        private static Document _doc;
        private static DwgNameDeleteVM _inputData;
        private static string _dwgName;
        private static int _countDwgDeleted;

        private const string TRANSACTION_NAME = "Delete DWG by Name";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                SortedDictionary<string, int> dwgList = GetDwgList();

                _inputData = GetUserInput(dwgList);
                if (_inputData == null)
                    return Result.Cancelled;

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    DeleteDwg();

                    trans.Commit();
                }
                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static SortedDictionary<string, int> GetDwgList()
        {
            // Get DWGs
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            IEnumerable<ImportInstance> dwgTypesAll = collector.OfClass(typeof(ImportInstance)).OrderBy(a => a.Name)
                .Cast<ImportInstance>(); //LINQ function;

            // Get unique imports names list
            List<ImportInstance> dwgTypes = new List<ImportInstance>();
            List<string> dwgTypesNames = new List<string>();
            foreach (ImportInstance i in dwgTypesAll)
            {
                string dwgTypeName = i.Category.Name;
                if (!dwgTypesNames.Contains(dwgTypeName))
                {
                    dwgTypes.Add(i);
                    dwgTypesNames.Add(dwgTypeName);
                }
            }

            SortedDictionary<string, int> dwgTypesList = new SortedDictionary<string, int>();
            foreach (ImportInstance i in dwgTypes)
            {
                dwgTypesList.Add(i.Category.Name, i.Id.IntegerValue);
            }

            return dwgTypesList;
        }

        private static DwgNameDeleteVM GetUserInput(SortedDictionary<string, int> dwgList)
        {
            DwgNameDeleteForm form = new DwgNameDeleteForm();
            DwgNameDeleteVM formVM = new DwgNameDeleteVM(dwgList);
            form.DataContext = formVM;
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            ElementId dwgId = new ElementId(_inputData.DwgListSelected);
            _dwgName = _doc?.GetElement(dwgId).Category.Name;

            // Get user provided information from window
            return form.DataContext as DwgNameDeleteVM;
        }

        private static void DeleteDwg()
        {
            ElementId dwgId = new ElementId(_inputData.DwgListSelected);
            string _dwgName = _doc?.GetElement(dwgId).Category.Name;

            // Get all Imports with name same as input from a form
            ICollection<ElementId> dwgDelete = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Where(x => x.Category.Name == _dwgName)
                .ToList()
                .ConvertAll(x => x.Id)
                .ToList();

            _doc.Delete(dwgDelete);

            _countDwgDeleted = dwgDelete.Count;
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countDwgDeleted == 0)
                ? "No DWG deleted"
                : $"{_countDwgDeleted} DWG named {_dwgName} deleted";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
        }
    }
}
