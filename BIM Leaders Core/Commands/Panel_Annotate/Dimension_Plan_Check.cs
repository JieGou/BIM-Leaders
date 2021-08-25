using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Dimension_Plan_Check : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                Options options = new Options
                {
                    ComputeReferences = true,
                    IncludeNonVisibleObjects = false,
                    View = view
                };

                Color color = new Color(255, 127, 39);

                double scale = view.Scale;
                XYZ zero = new XYZ(0,0,0);
                int count = 0;

                // Get Dimensions
                FilteredElementCollector collector_d = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> dimensions_all = collector_d.OfClass(typeof(Dimension))
                    .WhereElementIsNotElementType()
                    .ToElements();

                List<Reference> references_d = new List<Reference>();
                // Iterate through dimensions and get references
                foreach (Dimension d in dimensions_all)
                {
                    ReferenceArray ref_array = d.References;
                    foreach (Reference r in ref_array)
                    {
                        references_d.Add(r);
                    }
                }

                // Get Walls
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<Element> walls_all = collector.OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .ToElements();

                List<ElementId> wall_ids = new List<ElementId>();
                // Get Walls long sides references
                foreach (Wall w in walls_all)
                {
                    XYZ orient = w.Orientation;
                    foreach (Solid s in w.get_Geometry(options))
                    {
                        foreach (Face face in s.Faces)
                        {
                            // Some faces are cylidrical so pass them
                            try
                            {
                                // Check if face is vertical
                                UV p = new UV(0, 0);
                                if (Math.Round(face.ComputeNormal(p).Z) == 0)
                                {
                                    // Check if face perpendicular to wall normal
                                    if (Math.Round(face.ComputeNormal(p).AngleTo(orient)) == 0)
                                    {
                                        // Check if wall reference with no dimension
                                        Reference r = face.Reference;
                                        if (!references_d.Contains(r))
                                        {
                                            wall_ids.Add(w.Id);
                                            count++;
                                        }
                                    }
                                }
                            }
                            catch (Exception e) { }
                        }
                    }
                }

                using (Transaction trans = new Transaction(doc, "Create Filter for non-dimensioned Walls"))
                {
                    trans.Start();

                    // Checking if filter already exists
                    IEnumerable<Element> filters = new FilteredElementCollector(doc).OfClass(typeof(SelectionFilterElement)).ToElements();
                    foreach (Element e in filters)
                    {
                        if (e.Name == "Walls dimension filter")
                        {
                            doc.Delete(e.Id);
                        }
                    }

                    SelectionFilterElement filter = SelectionFilterElement.Create(doc, "Walls dimension filter");
                    filter.SetElementIds(wall_ids);

                    // Add the filter to the view
                    ElementId filterId = filter.Id;
                    view.AddFilter(filterId);
                    doc.Regenerate();

                    // Get solid pattern
                    IEnumerable<Element> patterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).ToElements();
                    ElementId pattern = patterns.First().Id;
                    foreach (Element e in patterns)
                    {
                        if (e.Name == "<Solid fill>")
                        {
                            pattern = e.Id;
                        }
                    }

                    // Use the existing graphics settings, and change the color to Orange
                    OverrideGraphicSettings overrideSettings = view.GetFilterOverrides(filterId);
                    overrideSettings.SetCutForegroundPatternColor(color);
                    overrideSettings.SetCutForegroundPatternId(pattern);
                    view.SetFilterOverrides(filterId, overrideSettings);

                    trans.Commit();
                }

                // Show result
                if (count == 0)
                {
                    TaskDialog.Show("Dimension Plan Check", "All walls are dimensioned");
                }
                else
                {
                    TaskDialog.Show("Dimension Plan Check", string.Format("{0} walls added to Walls dimension filter", wall_ids.Count.ToString()));
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
            return typeof(Dimension_Plan_Check).Namespace + "." + nameof(Dimension_Plan_Check);
        }
    }
}
