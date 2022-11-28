﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DwgNameDelete : BaseCommand
    {
        private Document _doc;
        private SortedDictionary<string, int> _dwgList;

        public DwgNameDelete()
        {
            _transactionName = "Delete DWG by Name";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult();
            _doc = commandData.Application.ActiveUIDocument.Document;
            _dwgList = GetDwgList();

            if (_dwgList.Count == 0)
            {
                _result.Started = true;
                _result.Result = "Document has no DWG.";
                ShowResult(_result);
                return Result.Failed;
            }

            Run(commandData);

            if (!_result.Started)
                return Result.Cancelled;
            if (_result.Failed)
                return Result.Failed;
            else
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

        private protected override void Run(ExternalCommandData commandData)
        {
            // Model
            DwgNameDeleteModel formM = new DwgNameDeleteModel(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DwgNameDeleteViewModel formVM = new DwgNameDeleteVM(formM)
            {
                DwgList = _dwgList,
                DwgListSelected = _dwgList.First().Value
            };

            // View
            DwgNameDeleteForm form = new DwgNameDeleteForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DwgNameDelete).Namespace + "." + nameof(DwgNameDelete);
    }
}