using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Elements_Join : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                int count = 0;

                //Plane view_plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
                // Solid of view section plane for filtering
                IList<CurveLoop> viewCrop = doc.ActiveView.GetCropRegionShapeManager().GetCropShape();
                Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(viewCrop, doc.ActiveView.ViewDirection, 1);
                ElementIntersectsSolidFilter eieFilter = new ElementIntersectsSolidFilter(s);

                // Get Walls Ids
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                ICollection<ElementId> wallsAll = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .WherePasses(eieFilter)
                    .ToElementIds();
                // Get Floors Ids
                FilteredElementCollector collector1 = new FilteredElementCollector(doc, doc.ActiveView.Id);
                ICollection<ElementId> floorsAll = collector1.OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .WherePasses(eieFilter)
                    .ToElementIds();

                // Get all Walls and Floors as Elements
                List<Element> elementsAll = new List<Element>();
                foreach(ElementId id in wallsAll)
                    elementsAll.Add(doc.GetElement(id));
                foreach (ElementId id in floorsAll)
                    elementsAll.Add(doc.GetElement(id));

                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    // Go through elements list and join all elements that close to each element
                    foreach (Element element in elementsAll)
                    {
                        // Filter for intersecting the current element
                        //ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(el);

                        BoundingBoxXYZ bb = element.get_BoundingBox(doc.ActiveView);
                        Outline outline = new Outline(bb.Min, bb.Max);
                        double tolerance = 0.1;
                        BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline, tolerance);

                        // List of elements that intersect
                        List<Element> elementsClose = new List<Element>();

                        // Collecting elements that intersect
                        FilteredElementCollector collectorW = new FilteredElementCollector(doc, wallsAll);
                        IList<Element> wallsClose = collectorW
                            .WherePasses(filter)
                            .ToElements();
                        FilteredElementCollector collectorF = new FilteredElementCollector(doc, floorsAll);
                        IList<Element> floorsClose = collectorF
                            .WherePasses(filter)
                            .ToElements();

                        // Joining
                        try
                        {
                            foreach (Element elementW in wallsClose)
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(doc, element, elementW))
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, element, elementW);
                                    count++;
                                }
                            }
                            foreach (Element elementF in floorsClose)
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(doc, element, elementF))
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, element, elementF);
                                    count++;
                                }
                            }
                        }
                        catch { }
                    }
                    trans.Commit();
                }

                // Show result
                if (count == 0)
                    TaskDialog.Show("Elements Join", "No joins found");
                else
                    TaskDialog.Show("Elements Join", string.Format("{0} elements cuts a view. {1} elements joins were done", elementsAll.Count.ToString(), count.ToString()));

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
            return typeof(Elements_Join).Namespace + "." + nameof(Elements_Join);
        }
    }
}