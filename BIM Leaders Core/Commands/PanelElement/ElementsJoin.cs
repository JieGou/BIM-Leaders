﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ElementsJoin : BaseCommand
    {
        private const double TOLERANCE = 0.1;

        private static Document _doc;
        private static int _countCutted;
        private static int _countJoined;

        public ElementsJoin()
        {
            _transactionName = "Join walls and floors on section";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                using (Transaction trans = new Transaction(_doc, _transactionName))
                {
                    trans.Start();

                    JoinElements();

                    trans.Commit();
                }

                _runResult = GetRunResult();

                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Join all elements that cut the currect section view.
        /// </summary>
        private static void JoinElements()
        {
            View view = _doc.ActiveView;

            ElementIntersectsSolidFilter intersectFilter = ViewUtils.GetViewCutIntersectFilter(view);

            // Get Walls Ids
            ICollection<ElementId> wallCutIds = new FilteredElementCollector(_doc, view.Id)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();
            // Get Floors Ids
            ICollection<ElementId> floorCutIds = new FilteredElementCollector(_doc, view.Id)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .WherePasses(intersectFilter)
                .ToElementIds();

            // Get all Walls and Floors as Elements
            List<Element> elementsCut = new List<Element>();
            foreach (ElementId id in wallCutIds)
                elementsCut.Add(_doc.GetElement(id));
            foreach (ElementId id in floorCutIds)
                elementsCut.Add(_doc.GetElement(id));

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
        private static void JoinElement(Element elementCut, ICollection<ElementId> elementCutIds)
        {
            try
            {
                BoundingBoxXYZ bb = elementCut.get_BoundingBox(_doc.ActiveView);
                Outline outline = new Outline(bb.Min, bb.Max);

                BoundingBoxIntersectsFilter intersectBoxFilter = new BoundingBoxIntersectsFilter(outline, TOLERANCE);

                // Apply filter to elements to find only elements that near the given element.
                IList<Element> elementsCutClose = new FilteredElementCollector(_doc, elementCutIds)
                    .WherePasses(intersectBoxFilter)
                    .ToElements();
                foreach (Element elementCutClose in elementsCutClose)
                    if (!JoinGeometryUtils.AreElementsJoined(_doc, elementCut, elementCutClose))
                    {
                        JoinGeometryUtils.JoinGeometry(_doc, elementCut, elementCutClose);
                        _countJoined++;
                    }
            }
            catch { }
        }

        private string GetRunResult()
        {
            string text = (_countJoined == 0)
                ? "No joins found."
                : $"{_countCutted} elements cuts a view. {_countJoined} elements joins were done.";

            return text;
        }

        private protected override void Run(ExternalCommandData commandData) { return; }

        public static string GetPath() => typeof(ElementsJoin).Namespace + "." + nameof(ElementsJoin);
    }
}