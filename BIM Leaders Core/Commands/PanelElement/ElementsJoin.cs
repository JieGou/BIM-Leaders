using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ElementsJoin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            double tolerance = 0.1;

            int countCutted = 0;
            int countJoined = 0;

            try
            {
                using (Transaction trans = new Transaction(doc, "Join walls and floors on section"))
                {
                    trans.Start();

                    (countCutted, countJoined) = JoinElements(doc, tolerance);

                    trans.Commit();
                }

                // Show result
                string text = (countJoined == 0)
                    ? "No joins found."
                    : $"{countCutted} elements cuts a view. {countJoined} elements joins were done.";
                TaskDialog.Show("Elements Join", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Join all elements that cut the currect section view.
        /// </summary>
        /// <returns>Count of elements that cuts a section and count of joined elements.</returns>
        private static (int, int) JoinElements(Document doc, double tolerance)
        {
            int countCutted = 0;
            int countJoined = 0;

            // Solid of view section plane for filtering
            IList<CurveLoop> viewCrop = doc.ActiveView.GetCropRegionShapeManager().GetCropShape();
            Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(viewCrop, doc.ActiveView.ViewDirection, 1);
            ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(s);

            // Get Walls Ids
            ICollection<ElementId> wallCutIds = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();
            // Get Floors Ids
            ICollection<ElementId> floorCutIds = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();

            // Get all Walls and Floors as Elements
            List<Element> elementsCut = new List<Element>();
            foreach (ElementId id in wallCutIds)
                elementsCut.Add(doc.GetElement(id));
            foreach (ElementId id in floorCutIds)
                elementsCut.Add(doc.GetElement(id));

            countCutted = elementsCut.Count;

            // Go through elements list and join all elements that close to each element
            foreach (Element elementCut in elementsCut)
            {
                countJoined += JoinElement(doc, tolerance, elementCut, wallCutIds);
                countJoined += JoinElement(doc, tolerance, elementCut, floorCutIds);
            }
            return (countCutted, countJoined);
        }

        /// <summary>
        /// Join element with set of elements. Also needs filter as input for better performance (to not calculate same filter couple of times).
        /// </summary>
        /// <returns>Count of joint elements.</returns>
        private static int JoinElement(Document doc, double tolerance, Element elementCut, ICollection<ElementId> elementCutIds)
        {
            int count = 0;
            try
            {
                BoundingBoxXYZ bb = elementCut.get_BoundingBox(doc.ActiveView);
                Outline outline = new Outline(bb.Min, bb.Max);

                BoundingBoxIntersectsFilter intersectBoxFilter = new BoundingBoxIntersectsFilter(outline, tolerance);

                // Apply filter to elements to find only elements that near the given element.
                IList<Element> elementsCutClose = new FilteredElementCollector(doc, elementCutIds)
                    .WherePasses(intersectBoxFilter)
                    .ToElements();
                foreach (Element elementCutClose in elementsCutClose)
                    if (!JoinGeometryUtils.AreElementsJoined(doc, elementCut, elementCutClose))
                    {
                        JoinGeometryUtils.JoinGeometry(doc, elementCut, elementCutClose);
                        count++;
                    }
            }
            catch { }
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementsJoin).Namespace + "." + nameof(ElementsJoin);
        }
    }
}