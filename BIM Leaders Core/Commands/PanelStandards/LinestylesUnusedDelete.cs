﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class LinestylesUnusedDelete : IExternalCommand
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
                // Get all used linestyles in the project.
                IEnumerable<ElementId> lineStylesUsed = new FilteredElementCollector(doc)
                    .OfClass(typeof(CurveElement))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<CurveElement>().ToList()
                    .ConvertAll(x => x.LineStyle.Id)
                    .Distinct();

                // Get all line styles in the project.
                CategoryNameMap lineStylesAllCnm = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
                List<ElementId> lineStylesAll = new List<ElementId>();
                foreach (Category category in lineStylesAllCnm)
                    lineStylesAll.Add(category.Id);

                List<ElementId> lineStylesDelete = lineStylesAll
                    .Where(x => !lineStylesUsed.Contains(x))
                    .ToList();

                count = lineStylesDelete.Count;

                using (Transaction trans = new Transaction(doc, "Delete Unused Linestyles"))
                {
                    trans.Start();

                    doc.Delete(lineStylesDelete);

                    trans.Commit();
                }

                // Show result
                string text = (count == 0)
                    ? "No linestyles deleted"
                    : $"{count} unused linestyles were deleted";
                TaskDialog.Show("Delete Unused Linestyles", text);

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
            return typeof(LinestylesUnusedDelete).Namespace + "." + nameof(LinestylesUnusedDelete);
        }
    }
}