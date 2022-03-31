using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class LinetypesDelete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int count = 0;

            try
            {
                LinetypesDeleteForm form = new LinetypesDeleteForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                LinetypesDeleteData data = form.DataContext as LinetypesDeleteData;

                string nameDelete = data.ResultName;

                List<Element> linePatterns = new FilteredElementCollector(doc)
                    .OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType()
                    .Where(x => x.Name.Contains(nameDelete))
                    .ToList();

                count = linePatterns.Count;

                using (Transaction trans = new Transaction(doc, "Delete Line Patterns"))
                {
                    trans.Start();

                    doc.Delete(linePatterns.ConvertAll(x => x.Id));

                    trans.Commit();
                }

                // Show result
                string text = count == 0
                    ? "No line patterns deleted"
                    : $"{count} line patterns deleted";
                TaskDialog.Show("Delete Line Patterns", text);

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
            return typeof(LinetypesDelete).Namespace + "." + nameof(LinetypesDelete);
        }
    }
}