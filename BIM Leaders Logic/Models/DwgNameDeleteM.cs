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
    public class DwgNameDeleteM : BaseModel
    {
        private string _dwgName;
        private int _countDwgDeleted;

        #region PROPERTIES

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

        public DwgNameDeleteM(ExternalCommandData commandData, string transactionName) : base(commandData, transactionName)
        {
        }

        #region IEXTERNALEVENTHANDLER

        public override void Execute(UIApplication app)
        {
            RunStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, TransactionName))
                {
                    trans.Start();

                    DeleteDwg();

                    trans.Commit();
                }

                RunResult = GetRunResult();
            }
            catch (Exception e)
            {
                RunFailed = true;
                RunResult = ExceptionUtils.GetMessage(e);
            }
        }

        #endregion

        #region METHODS

        private void DeleteDwg()
        {
            ElementId dwgId = new ElementId(DwgListSelected);
            _dwgName = _doc?.GetElement(dwgId).Category.Name;

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

        private protected override string GetRunResult()
        {
            string text = (_countDwgDeleted == 0)
                ? "No DWG deleted"
                : $"{_countDwgDeleted} DWG named {_dwgName} deleted";

            return text;
        }

        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages) { return null; }

        #endregion
    }
}