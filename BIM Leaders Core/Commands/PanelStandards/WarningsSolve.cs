using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class WarningsSolve : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int countJoins = 0;

            try
            {
                // Now solve only joins, so no input window results
                bool joins = true;

                IEnumerable<FailureMessage> warnings = doc.GetWarnings();

                IEnumerable<FailureMessage> warningsJoin = warnings
                    .Where(x => x.GetDescriptionText() == "Highlighted elements are joined but do not intersect.");

                using (Transaction trans = new Transaction(doc, "Solve Warnings"))
                {
                    trans.Start();

                    if (joins)
                        countJoins = SolveJoins(doc, warningsJoin);
                    
                    trans.Commit();
                }

                // Show result
                string text = (countJoins == 0)
                    ? "No warnings to solve"
                    : $"{countJoins} wrong joins were removed";
                TaskDialog.Show("Solve Warnings", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Unjoin elements that have a warning about joining.
        /// </summary>
        /// <returns>Count of solved warnings.</returns>
        private static int SolveJoins(Document doc, IEnumerable<FailureMessage> warningsJoin)
        {
            int count = 0;
            foreach (FailureMessage warning in warningsJoin)
            {
                List<ElementId> ids = warning.GetFailingElements().ToList();

                // Filter elements in workshared document that are editable
                if (doc.IsWorkshared)
                    ids = WorksharingUtils.CheckoutElements(doc, ids).ToList();
                if (ids.Count < 2)
                    continue;

                Element element0 = doc.GetElement(ids[0]);
                Element element1 = doc.GetElement(ids[1]);

                try
                {
                    JoinGeometryUtils.UnjoinGeometry(doc, element0, element1);
                    count++;
                }
                catch { }
            }
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}
