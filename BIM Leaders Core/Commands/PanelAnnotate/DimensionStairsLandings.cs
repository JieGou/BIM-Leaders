using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionStairsLandings : IExternalCommand
    {
        private static Document _doc;

        private const string TRANSACTION_NAME = "Annotate Landings";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            CheckViewDepth();

            // Models
            DimensionStairsLandingsM formM = new DimensionStairsLandingsM(commandData);
            ExternalEvent externalEvent = ExternalEvent.Create(formM);
            formM.ExternalEvent = externalEvent;

            // ViewModel
            DimensionStairsLandingsVM formVM = new DimensionStairsLandingsVM(formM);

            // View
            DimensionStairsLandingsForm form = new DimensionStairsLandingsForm() { DataContext = formVM };
            form.ShowDialog();

            if (form.DialogResult == false)
                return Result.Cancelled;

            return Result.Succeeded;
        }

        private static void CheckViewDepth()
        {
            double allowableViewDepth = 1;

            View view = _doc.ActiveView;
            double viewDepth = view.get_Parameter(BuiltInParameter.VIEWER_BOUND_OFFSET_FAR).AsDouble();

            if (viewDepth > allowableViewDepth)
                TaskDialog.Show("Dimension Stairs", "View depth is too high. This may cause errors. Set far clip offset at most 30 cm.", TaskDialogCommonButtons.Ok);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(DimensionStairsLandings).Namespace + "." + nameof(DimensionStairsLandings);
        }
    }
}
