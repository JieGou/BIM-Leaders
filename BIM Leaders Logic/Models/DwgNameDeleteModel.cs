using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class DwgNameDeleteModel : BaseModel
    {
        private string _dwgName;
        private int _countDwgDeleted;

        #region PROPERTIES

        private SortedDictionary<string, int> _dwgList;
        public SortedDictionary<string, int> DwgList
        {
            get { return _dwgList; }
            set
            {
                _dwgList = value;
                OnPropertyChanged(nameof(DwgList));
            }
        }

        private int _dwgListSelected;
        public int DwgListSelected
        {
            get { return _dwgListSelected; }
            set
            {
                _dwgListSelected = value;
                OnPropertyChanged(nameof(DwgListSelected));
            }
        }

        #endregion

        #region METHODS

        public override void SetInitialData()
        {
            DwgList = GetDwgList();
            DwgListSelected = DwgList.First().Value;
        }

        private SortedDictionary<string, int> GetDwgList()
        {
            IEnumerable<ImportInstance> dwgTypesAll = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImportInstance))
                .OrderBy(a => a.Name)
                .Cast<ImportInstance>();

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

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                DeleteDwg();

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        private void DeleteDwg()
        {
            ElementId dwgId = new ElementId(DwgListSelected);
            _dwgName = Doc?.GetElement(dwgId).Category.Name;

            // Get all Imports with name same as input from a form
            ICollection<ElementId> dwgDelete = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Where(x => x.Category.Name == _dwgName)
                .ToList()
                .ConvertAll(x => x.Id)
                .ToList();

            Doc.Delete(dwgDelete);

            _countDwgDeleted = dwgDelete.Count;
        }

        private protected override string GetRunResult()
        {
            string text = (_countDwgDeleted == 0)
                ? "No DWG deleted"
                : $"{_countDwgDeleted} DWG named {_dwgName} deleted";

            return text;
        }

        #endregion
    }
}