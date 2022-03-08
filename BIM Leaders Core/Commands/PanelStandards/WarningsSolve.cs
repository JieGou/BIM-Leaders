using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

            try
            {
                /*
                // Collector for data provided in window
                Checker_Data data = new Checker_Data();

                Checker_Form form = new Checker_Form();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Checker_Data;

                // Getting input data from user
                string prefix = data.result_prefix;
                List<bool> categories = data.result_categories;
                List<bool> model = data.result_model;
                List<bool> codes = data.result_codes;
                int head_height = data.result_height;

                */

                // now solve only joins, so no input window results

                bool joins = true;
                int countJoins = 0;


                IList<FailureMessage> warnings = doc.GetWarnings();



                using (Transaction trans = new Transaction(doc, "Solve Warnings"))
                {
                    trans.Start();

                    if (joins)
                        foreach (FailureMessage warning in warnings)
                            if (warning.GetDescriptionText() == "Highlighted elements are joined but do not intersect.")
                            {
                                List<ElementId> ids = warning.GetFailingElements().ToList();
                                Element element0 = doc.GetElement(ids[0]);
                                Element element1 = doc.GetElement(ids[1]);

                                JoinGeometryUtils.UnjoinGeometry(doc, element0, element1);

                                countJoins++;
                            }
                    
                    trans.Commit();

                    // Show result
                    if (countJoins == 0)
                        TaskDialog.Show("Solve Warnings", "No warnings to solve");
                    else
                        TaskDialog.Show("Solve Warnings", string.Format("{0} wrong joins were removed", countJoins.ToString()));

                    return Result.Succeeded;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(WarningsSolve).Namespace + "." + nameof(WarningsSolve);
        }
    }
}
