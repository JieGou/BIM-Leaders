using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ElementsJoin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            Document doc = commandData.Application.ActiveUIDocument.Document;

            double tolerance = 0.1;
            
            int countCutted = 0;
            int countJoined = 0;

            try
            {
                using (Transaction trans = new Transaction(doc, "Join walls and floors on section"))
                {
                    trans.Start();

                    JoinElements(doc, tolerance, ref countCutted, ref countJoined);

                    trans.Commit();
                }
                ShowResult(countCutted, countJoined);

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
        private static void JoinElements(Document doc, double tolerance, ref int countCutted, ref int countJoined)
        {
            View view = doc.ActiveView;

            ElementIntersectsSolidFilter intersectFilter = ViewUtils.GetViewCutIntersectFilter(view);

            // Get Walls Ids
            ICollection<ElementId> wallCutIds = new FilteredElementCollector(doc, view.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();
            // Get Floors Ids
            ICollection<ElementId> floorCutIds = new FilteredElementCollector(doc, view.Id)
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
                JoinElement(doc, tolerance, elementCut, wallCutIds, ref countJoined);
                JoinElement(doc, tolerance, elementCut, floorCutIds, ref countJoined);
            }
        }

        /// <summary>
        /// Join element with set of elements. Also needs filter as input for better performance (to not calculate same filter couple of times).
        /// </summary>
        private static void JoinElement(Document doc, double tolerance, Element elementCut, ICollection<ElementId> elementCutIds, ref int count)
        {
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
        }

        private static void ShowResult(int countCutted, int countJoined)
        {
            // Show result
            string text = (countJoined == 0)
                ? "No joins found."
                : $"{countCutted} elements cuts a view. {countJoined} elements joins were done.";
            
            TaskDialog.Show("Elements Join", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementsJoin).Namespace + "." + nameof(ElementsJoin);
        }
    }
}