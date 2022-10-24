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
        private Document _doc;

        private const string TRANSACTION_NAME = "Compare Walls";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;
            SortedDictionary<string, int> materials = GetListMaterials();
            SortedDictionary<string, int> fillTypes = GetListFillTypes();
            if (materials.Count == 0)
            {
                TaskDialog.Show(TRANSACTION_NAME, "Document has no materials.");
                return Result.Failed;
            }
            if (fillTypes.Count == 0)
            {
                TaskDialog.Show(TRANSACTION_NAME, "Document has no fill types.");
                return Result.Failed;
            }

            // Model
            WallsCompareM formM = new WallsCompareM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            WallsCompareVM formVM = new WallsCompareVM(formM);

            formVM.Materials = materials;
            formVM.FillTypes = fillTypes;
            formVM.MaterialsSelected = materials.First().Value;
            formVM.FillTypesSelected = fillTypes.First().Value;

            // View
            WallsCompareForm form = new WallsCompareForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

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

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WallsCompare).Namespace + "." + nameof(WallsCompare);
        }
    }
}