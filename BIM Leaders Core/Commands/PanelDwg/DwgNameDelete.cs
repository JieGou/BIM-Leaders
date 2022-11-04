using System.Threading.Tasks;
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
        private SortedDictionary<string, int> _dwgList;

        private const string TRANSACTION_NAME = "Delete DWG by Name";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _dwgList = GetDwgList();

            if (_dwgList.Count == 0)
            {
                ShowResult("Document has no DWG.");
                return Result.Failed;
            }

            Run(commandData);

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

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            DwgNameDeleteM formM = new DwgNameDeleteM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DwgNameDeleteVM formVM = new DwgNameDeleteVM(formM);

            formVM.DwgList = _dwgList;
            formVM.DwgListSelected = _dwgList.First().Value;

            // View
            DwgNameDeleteForm form = new DwgNameDeleteForm(formVM) { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            if (formM.RunResult.Length > 0)
                ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
        }
    }
}