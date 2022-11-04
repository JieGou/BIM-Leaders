using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using BIM_Leaders_Logic;
using System.Threading.Tasks;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionStairsLandings : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Annotate Landings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            CheckViewDepth(commandData);

            Run(commandData);

            return Result.Succeeded;
        }

        private void CheckViewDepth(ExternalCommandData commandData)
        {
            double allowableViewDepth = 1;

            View view = commandData.Application.ActiveUIDocument.Document.ActiveView;
            double viewDepth = view.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).AsDouble();

            if (viewDepth > allowableViewDepth)
                ShowResult("View depth is too high. This may cause errors. Set far clip offset at most 30 cm.");
        }

        private async void Run(ExternalCommandData commandData)
        {
            // Models
            DimensionStairsLandingsM formM = new DimensionStairsLandingsM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionStairsLandingsVM formVM = new DimensionStairsLandingsVM(formM);

            // View
            DimensionStairsLandingsForm form = new DimensionStairsLandingsForm(formVM) { DataContext = formVM };
            form.ShowDialog();

            await Task.Delay(1000);

            ShowResult(formM.RunResult);
        }

        private void ShowResult(string resultText)
        {
            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
        }
    }
}