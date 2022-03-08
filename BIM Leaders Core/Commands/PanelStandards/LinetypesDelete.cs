﻿using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
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

            try
            {
                // Collector for data provided in window
                LinetypesDeleteData data = new LinetypesDeleteData();

                LinetypesDeleteForm form = new LinetypesDeleteForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as LinetypesDeleteData;

                string nameDelete = data.ResultName;
                int count = 0;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<LinePatternElement> linePatterns = collector.OfClass(typeof(LinePatternElement))
                    .WhereElementIsNotElementType().ToElements().Cast<LinePatternElement>();

                using (Transaction trans = new Transaction(doc, "Delete Line Patterns"))
                {
                    trans.Start();

                    // Deleting unused line patterns
                    foreach (LinePatternElement linePattern in linePatterns)
                    {
                        string linePatternName = linePattern.Name;

                        if (linePatternName.Contains(nameDelete))
                        {
                            ElementId linePatternId = linePattern.Id;
                            doc.Delete(linePatternId);
                            count++;
                        }
                    }

                    trans.Commit();
                }

                // Show result
                if (count == 0)
                    TaskDialog.Show("Delete Line Patterns", "No line patterns deleted");
                else
                    TaskDialog.Show("Delete Line Patterns", string.Format("{0} line patterns deleted", count.ToString()));

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