using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class ElementsJoinModel : BaseModel
    {
        private const double TOLERANCE = 0.1;

        private int _countCutted;
        private int _countJoined;

        #region PROPERTIES

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                JoinElements();

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        /// <summary>
        /// Join all elements that cut the currect section view.
        /// </summary>
        private void JoinElements()
        {
            View view = Doc.ActiveView;

            ElementIntersectsSolidFilter intersectFilter = ViewUtils.GetViewCutIntersectFilter(view);

            // Get Walls Ids
            ICollection<ElementId> wallCutIds = new FilteredElementCollector(Doc, view.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();
            // Get Floors Ids
            ICollection<ElementId> floorCutIds = new FilteredElementCollector(Doc, view.Id)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();

            // Get all Walls and Floors as Elements
            List<Element> elementsCut = new List<Element>();
            foreach (ElementId id in wallCutIds)
                elementsCut.Add(Doc.GetElement(id));
            foreach (ElementId id in floorCutIds)
                elementsCut.Add(Doc.GetElement(id));

            _countCutted = elementsCut.Count;

            // Go through elements list and join all elements that close to each element
            foreach (Element elementCut in elementsCut)
            {
                JoinElement(elementCut, wallCutIds);
                JoinElement(elementCut, floorCutIds);
            }
        }

        /// <summary>
        /// Join element with set of elements. Also needs filter as input for better performance (to not calculate same filter couple of times).
        /// </summary>
        private void JoinElement(Element elementCut, ICollection<ElementId> elementCutIds)
        {
            try
            {
                BoundingBoxXYZ bb = elementCut.get_BoundingBox(Doc.ActiveView);
                Outline outline = new Outline(bb.Min, bb.Max);

                BoundingBoxIntersectsFilter intersectBoxFilter = new BoundingBoxIntersectsFilter(outline, TOLERANCE);

                // Apply filter to elements to find only elements that near the given element.
                IList<Element> elementsCutClose = new FilteredElementCollector(Doc, elementCutIds)
                    .WherePasses(intersectBoxFilter)
                    .ToElements();
                foreach (Element elementCutClose in elementsCutClose)
                    if (!JoinGeometryUtils.AreElementsJoined(Doc, elementCut, elementCutClose))
                    {
                        JoinGeometryUtils.JoinGeometry(Doc, elementCut, elementCutClose);
                        _countJoined++;
                    }
            }
            catch { }
        }

        private protected override string GetRunResult()
        {
            string text = (_countJoined == 0)
                ? "No joins found."
                : $"{_countCutted} elements cuts a view. {_countJoined} elements joins were done.";

            return text;
        }

        #endregion
    }
}