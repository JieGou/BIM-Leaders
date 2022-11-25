﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionStairsLandings : BaseCommand
    {
        public DimensionStairsLandings()
        {
            _transactionName = "Annotate Landings";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CheckViewDepth(commandData);

            Run(commandData);

            if (!_runStarted)
                return Result.Cancelled;
            if (_runFailed)
                return Result.Failed;
            else
                return Result.Succeeded;
        }

        private void CheckViewDepth(ExternalCommandData commandData)
        {
            double allowableViewDepth = 1;

            View view = commandData.Application.ActiveUIDocument.Document.ActiveView;
            double viewDepth = view.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).AsDouble();

            if (viewDepth > allowableViewDepth)
            {
                _runResult = "View depth is too high. This may cause errors. Set far clip offset at most 30 cm.";
                ShowResult();
            }
        }

        private protected override void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionStairsLandingsM formM = new DimensionStairsLandingsM(commandData, _transactionName, ShowResult);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionStairsLandingsVM formVM = new DimensionStairsLandingsVM(formM);

            // View
            DimensionStairsLandingsForm form = new DimensionStairsLandingsForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath() => typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
    }
}