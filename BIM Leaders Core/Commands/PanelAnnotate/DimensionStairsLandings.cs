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

            _model = new DimensionStairsLandingsModel();
            _viewModel = new DimensionStairsLandingsViewModel();
            _view = new DimensionStairsLandingsForm();
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult() { Started = true };

            CheckViewDepth(commandData);

            Run(commandData);

            if (!_result.Started)
                return Result.Cancelled;
            if (_result.Failed)
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
                _result.Result = "View depth is too high. This may cause errors. Set far clip offset at most 30 cm.";
                ShowResult(_result);
            }
        }

        public static string GetPath() => typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
    }
}