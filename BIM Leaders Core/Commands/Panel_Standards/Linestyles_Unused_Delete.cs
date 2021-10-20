using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Core
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
                IEnumerable<CurveElement> usedLines = collector.OfClass(typeof(CurveElement))
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<CurveElement>();

                List<ElementId> lineStylesUsed = new List<ElementId>();

                foreach(CurveElement usedLine in usedLines)
                {
                    ElementId lineStyle = usedLine.LineStyle.Id;

                    if(!lineStylesUsed.Contains(lineStyle))
                        lineStylesUsed.Add(lineStyle);
                }

                // Selecting all line styles in the project
                CategoryNameMap lineStylesAllCnm = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
                List<ElementId> lineStylesAll = new List<ElementId>();
                foreach (Category category in lineStylesAllCnm)
                    lineStylesAll.Add(category.Id);

                List<ElementId> lineStyles = new List<ElementId>();
                foreach(ElementId lineStyle in lineStylesAll)
                    lineStyles.Add(lineStyle);

                int count = 0;

                using (Transaction trans = new Transaction(doc, "Delete Unused Linestyles"))
                {
                    trans.Start();

                    // Deleting unused line styles
                    foreach (ElementId lineStyle in lineStyles)
                    {
                        if (!lineStylesUsed.Contains(lineStyle))
                        {
                            try
                            {
                                doc.Delete(lineStyle);
                                count++;
                            }
                            catch { }
                        }
                    }

                    trans.Commit();
                }

                // Show result
                if (count == 0)
                    TaskDialog.Show("Delete Unused Linestyles", "No linestyles deleted");
                else
                    TaskDialog.Show("Delete Unused Linestyles", string.Format("{0} unused linestyles were deleted", count.ToString()));

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
            return typeof(Linestyles_Unused_Delete).Namespace + "." + nameof(Linestyles_Unused_Delete);
        }
    }
}