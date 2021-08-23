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
                IList<CurveLoop> view_crop = doc.ActiveView.GetCropRegionShapeManager().GetCropShape();
                Solid s = GeometryCreationUtilities.CreateExtrusionGeometry(view_crop, doc.ActiveView.ViewDirection, 1);
                ElementIntersectsSolidFilter eief = new ElementIntersectsSolidFilter(s);

                // Get Walls Ids
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                ICollection<ElementId> walls_all = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .WherePasses(eief)
                    .ToElementIds();
                // Get Floors Ids
                FilteredElementCollector collector_1 = new FilteredElementCollector(doc, doc.ActiveView.Id);
                ICollection<ElementId> floors_all = collector_1.OfClass(typeof(Floor))
                    .WhereElementIsNotElementType()
                    .WherePasses(eief)
                    .ToElementIds();

                // Get all Walls and Floors as Elements
                List<Element> elements_all = new List<Element>();
                foreach(ElementId id in walls_all)
                {
                    elements_all.Add(doc.GetElement(id));
                }
                foreach (ElementId id in floors_all)
                {
                    elements_all.Add(doc.GetElement(id));
                }

                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    // Go through elements list and join all elements that close to each element
                    foreach (Element el in elements_all)
                    {
                        // Filter for intersecting the current element
                        //ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(el);

                        BoundingBoxXYZ bb = el.get_BoundingBox(doc.ActiveView);
                        Outline outline = new Outline(bb.Min, bb.Max);
                        double tolerance = 0.1;
                        BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline, tolerance);

                        // List of elements that intersect
                        List<Element> elements_close = new List<Element>();

                        // Collecting elements that intersect
                        FilteredElementCollector collector_w = new FilteredElementCollector(doc, walls_all);
                        IList<Element> walls_close = collector_w
                            .WherePasses(filter)
                            .ToElements();
                        FilteredElementCollector collector_f = new FilteredElementCollector(doc, floors_all);
                        IList<Element> floors_close = collector_w
                            .WherePasses(filter)
                            .ToElements();

                        // Joining
                        try
                        {
                            foreach (Element el_w in walls_close)
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(doc, el, el_w))
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, el, el_w);
                                    count++;
                                }
                            }
                            foreach (Element el_f in floors_close)
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(doc, el, el_f))
                                {
                                    JoinGeometryUtils.JoinGeometry(doc, el, el_f);
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
                {
                    TaskDialog.Show("Elements Join", "No joins found");
                }
                else
                {
                    TaskDialog.Show("Elements Join", string.Format("{0} elements cuts a view. {1} elements joins were done", elements_all.Count.ToString(), count.ToString()));
                }

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