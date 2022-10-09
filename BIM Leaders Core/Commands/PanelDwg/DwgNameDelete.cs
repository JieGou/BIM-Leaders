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
        private static DwgNameDeleteData _inputData;
        private static string _dwgName = _doc?.GetElement(_inputData.DwgListSelected).Category.Name;
        private static int _countDwgDeleted;

        private const string TRANSACTION_NAME = "Delete DWG by Name";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _inputData = GetUserInput();
            if (_inputData == null)
                return Result.Cancelled;

                // Get user provided information from window
                DwgNameDeleteData data = form.DataContext as DwgNameDeleteData;

                string name = doc.GetElement(data.DwgListSelected).Category.Name;

                // Get all Imports with name same as input from a form
                ICollection<ElementId> dwgDelete = new FilteredElementCollector(doc)
                    .OfClass(typeof(ImportInstance))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Category.Name == name) //LINQ function
                    .ToList()                            //LINQ function
                    .ConvertAll(x => x.Id)               //LINQ function
                    .ToList();                           //LINQ function

                using (Transaction trans = new Transaction(doc, "Delete DWG by Name"))
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

        private static DwgNameDeleteData GetUserInput()
        {
            DwgNameDeleteForm form = new DwgNameDeleteForm(_doc);
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as DwgNameDeleteData;
        }

        private static void DeleteDwg()
        {
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

        private static void ShowResult(int count, string name)
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
