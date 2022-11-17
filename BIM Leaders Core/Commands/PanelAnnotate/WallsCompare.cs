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
    public class WallsCompare : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Compare Walls";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private Document _doc;
        private SortedDictionary<string, int> _materials;
        private SortedDictionary<string, int> _fillTypes;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            _materials = GetListMaterials();
            _fillTypes = GetListFillTypes();

            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private SortedDictionary<string, int> GetListMaterials()
        {
            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            IEnumerable<Material> materialsAll = collector.OfClass(typeof(Material)).OrderBy(a => a.Name)
                .Cast<Material>(); //LINQ function;

            // Get unique fills names list
            List<Material> materials = new List<Material>();
            List<string> materialsNames = new List<string>();
            foreach (Material i in materialsAll)
            {
                string materialName = i.Name;
                if (!materialsNames.Contains(materialName))
                {
                    materials.Add(i);
                    materialsNames.Add(materialName);
                }
            }

            SortedDictionary<string, int> materialsList = new SortedDictionary<string, int>();
            foreach (Material i in materials)
            {
                materialsList.Add(i.Name, i.Id.IntegerValue);
            }

            return materialsList;
        }

        private SortedDictionary<string, int> GetListFillTypes()
        {
            // Get Fills
            FilteredElementCollector collector = new FilteredElementCollector(_doc);
            IEnumerable<FilledRegionType> fillTypesAll = collector.OfClass(typeof(FilledRegionType)).OrderBy(a => a.Name)
                .Cast<FilledRegionType>(); //LINQ function;

            // Get unique fills names list
            List<FilledRegionType> fillTypes = new List<FilledRegionType>();
            List<string> fillTypesNames = new List<string>();
            foreach (FilledRegionType i in fillTypesAll)
            {
                string fillTypeName = i.Name;
                if (!fillTypesNames.Contains(fillTypeName))
                {
                    fillTypes.Add(i);
                    fillTypesNames.Add(fillTypeName);
                }
            }

            //List<KeyValuePair<string, ElementId>> list = new List<KeyValuePair<string, ElementId>>();
            SortedDictionary<string, int> fillTypesList = new SortedDictionary<string, int>();
            foreach (FilledRegionType i in fillTypes)
            {
                fillTypesList.Add(i.Name, i.Id.IntegerValue);
            }

            return fillTypesList;
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Model
            WallsCompareM formM = new WallsCompareM(commandData, TRANSACTION_NAME);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WallsCompareVM formVM = new WallsCompareVM(formM)
            {
                Materials = _materials,
                FillTypes = _fillTypes,
                MaterialsSelected = _materials.First().Value,
                FillTypesSelected = _fillTypes.First().Value
            };

            // View
            WallsCompareForm form = new WallsCompareForm() { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            _runStarted = formM.RunStarted;
            _runFailed = formM.RunFailed;
            _runResult = formM.RunResult;

            ShowResult();
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, _runResult);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
        }
    }
}