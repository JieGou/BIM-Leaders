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
    public class DwgViewFoundModel : BaseModel
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

        #region METHODS

        private protected override void TryExecute()
        {
            // Get Imports
            IEnumerable<ImportInstance> imports = new FilteredElementCollector(Doc)
                .OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType()
                .Cast<ImportInstance>();

            int dwgId = 0;
            if (!Int32.TryParse(SelectedDwg, out dwgId))
            {
                Result.Result = "Error getting a DWG from the selected item.";
                TaskDialog.Show(TransactionName, Result.Result);
                return;
            }

            Element dwg = Doc.GetElement(new ElementId(dwgId));
            List<ElementId> selectionSet = new List<ElementId>() { new ElementId(dwgId) };

            if (dwg.ViewSpecific)
            {
                View view = Doc.GetElement(dwg.OwnerViewId) as View;
                Uidoc.ActiveView = view;
            }

            Uidoc.Selection.SetElementIds(selectionSet);
        }

        #endregion
    }
}