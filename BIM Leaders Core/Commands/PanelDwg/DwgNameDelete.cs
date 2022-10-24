using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DwgNameDelete : IExternalCommand
    {
        private Document _doc;

        private const string TRANSACTION_NAME = "Delete DWG by Name";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            SortedDictionary<string, int> dwgList = GetDwgList();
            if (dwgList.Count == 0)
            {
                TaskDialog.Show(TRANSACTION_NAME, "Document has no DWG.");
                return Result.Failed;
            }

            // Model
            DwgNameDeleteM formM = new DwgNameDeleteM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DwgNameDeleteVM formVM = new DwgNameDeleteVM(formM);

            formVM.DwgList = dwgList;
            formVM.DwgListSelected = dwgList.First().Value;

            // View
            DwgNameDeleteForm form = new DwgNameDeleteForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        private SortedDictionary<string, int> GetDwgList()
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

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
        }
    }
}
