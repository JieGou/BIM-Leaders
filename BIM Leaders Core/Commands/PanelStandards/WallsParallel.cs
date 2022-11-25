﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class WallsParallel : BaseCommand
    {
        public WallsParallel()
        {
            _transactionName = "Walls Parallel Check";
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            WallsParallelM formM = new WallsParallelM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;
            SelectReferencePlaneM formSelectionM = new SelectReferencePlaneM(commandData);

            // ViewModel
            WallsParallelVM formVM = new WallsParallelVM(formM, formSelectionM);

            // View
            WallsParallelForm form = new WallsParallelForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(WallsParallel).Namespace + "." + nameof(WallsParallel);
    }
}