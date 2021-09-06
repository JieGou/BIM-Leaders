using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Linetypes_Delete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                // Collector for data provided in window
                Linetypes_Delete_Data data = new Linetypes_Delete_Data();

                Linetypes_Delete_Form form = new Linetypes_Delete_Form();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Linetypes_IMPORT_Delete_Data;

                string name_delete = data.result_name;
                int count = 0;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<LinePatternElement> line_patterns = collector.OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType().ToElements().Cast<LinePatternElement>();

                using (Transaction trans = new Transaction(doc, "Delete Line Patterns"))
                {
                    trans.Start();

                    // Deleting unused line patterns
                    foreach (LinePatternElement l in line_patterns)
                    {
                        string name = l.Name;

                        if (name.Contains(name_delete))
                        {
                            ElementId id = l.Id;
                            doc.Delete(id);
                            count++;
                        }
                    }

                    trans.Commit();
                }

                // Show result
                if (count == 0)
                {
                    TaskDialog.Show("Delete Line Patterns", "No line patterns deleted");
                }
                else
                {
                    TaskDialog.Show("Delete Line Patterns", string.Format("{0} line patterns deleted", count.ToString()));
                }

                return Result.Succeeded;
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
            return typeof(Linetypes_Delete).Namespace + "." + nameof(Linetypes_IMPORT_Delete);
        }
    }
}