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
    public class DwgViewFoundM : BaseModel
    {
        #region PROPERTIES

        private string _selectedDwg;
        public string SelectedDwg
        {
            get { return _selectedDwg; }
            set
            {
                _selectedDwg = value;
                OnPropertyChanged(nameof(SelectedDwg));
            }
        }

        #endregion

        public DwgViewFoundM(
            ExternalCommandData commandData,
            string transactionName,
            Action<RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            // Get Imports
            IEnumerable<ImportInstance> imports = new FilteredElementCollector(_doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>();

            int dwgId = 0;
            if (!Int32.TryParse(SelectedDwg, out dwgId))
            {
                _result.Result = "Error getting a DWG from the selected item.";
                TaskDialog.Show(TransactionName, _result.Result);
                return;
            }

            Element dwg = _doc.GetElement(new ElementId(dwgId));
            List<ElementId> selectionSet = new List<ElementId>() { new ElementId(dwgId) };

            if (dwg.ViewSpecific)
            {
                View view = _doc.GetElement(dwg.OwnerViewId) as View;
                _uidoc.ActiveView = view;
            }

            _uidoc.Selection.SetElementIds(selectionSet);
        }

        #endregion
    }
}