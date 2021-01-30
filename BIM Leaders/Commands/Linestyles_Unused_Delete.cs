using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Linestyles_Unused_Delete : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                IEnumerable<CurveElement> lines_used = collector.OfClass(typeof(CurveElement))
                    .WhereElementIsNotElementType().ToElements().Cast<CurveElement>();

                List<ElementId> line_styles_used = new List<ElementId>();

                foreach(CurveElement l in lines_used)
                {
                    ElementId line_style = l.LineStyle.Id;

                    if(!line_styles_used.Contains(line_style))
                    {
                        line_styles_used.Add(line_style);
                    }
                }

                // Selecting all line styles in the project
                CategoryNameMap line_styles_all_cnm = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
                List<ElementId> line_styles_all = new List<ElementId>();
                foreach (Category c in line_styles_all_cnm)
                {
                    line_styles_all.Add(c.Id);
                }

                List<ElementId> line_styles = new List<ElementId>();
                foreach(ElementId i in line_styles_all)
                {
                    line_styles.Add(i);
                }

                int count = 0;

                using (Transaction trans = new Transaction(doc, "Delete Unused Linestyles"))
                {
                    trans.Start();

                    // Deleting unused line styles
                    foreach (ElementId i in line_styles)
                    {
                        if (!line_styles_used.Contains(i))
                        {
                            try
                            {
                                doc.Delete(i);
                                count++;
                            }
                            catch
                            {

                            }
                        }
                    }

                    trans.Commit();
                }

                // Show result
                if (count == 0)
                {
                    TaskDialog.Show("Delete Unused Linestyles", "No linestyles deleted");
                }
                else
                {
                    TaskDialog.Show("Delete Unused Linestyles", string.Format("{0} unused linestyles were deleted", count.ToString()));
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}