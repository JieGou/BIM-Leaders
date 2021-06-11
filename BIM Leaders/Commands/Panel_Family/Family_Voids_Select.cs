using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class Family_Voids_Select : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                Family family = doc.OwnerFamily;

                Options o = new Options();
                o.ComputeReferences = true;
                o.IncludeNonVisibleObjects = true;

                // Get Geometry primitives
                IList<Element> prim_ext = new FilteredElementCollector(doc).OfClass(typeof(Extrusion)).ToElements();
                IList<Element> prim_bln = new FilteredElementCollector(doc).OfClass(typeof(Blend)).ToElements();
                IList<Element> prim_rev = new FilteredElementCollector(doc).OfClass(typeof(Revolution)).ToElements();
                IList<Element> prim_swe = new FilteredElementCollector(doc).OfClass(typeof(Sweep)).ToElements();
                IList<Element> prim_swb = new FilteredElementCollector(doc).OfClass(typeof(SweptBlend)).ToElements();

                // Find Voids
                List<Element> voids = new List<Element>();
                foreach(GenericForm form in prim_ext)
                {
                    if(!form.IsSolid)
                    {
                        voids.Add(form);
                    }
                }
                foreach (GenericForm form in prim_bln)
                {
                    if (!form.IsSolid)
                    {
                        voids.Add(form);
                    }
                }
                foreach (GenericForm form in prim_rev)
                {
                    if (!form.IsSolid)
                    {
                        voids.Add(form);
                    }
                }
                foreach (GenericForm form in prim_swe)
                {
                    if (!form.IsSolid)
                    {
                        voids.Add(form);
                    }
                }
                foreach (GenericForm form in prim_swb)
                {
                    if (!form.IsSolid)
                    {
                        voids.Add(form);
                    }
                }

                // Selection
                List<ElementId> ids = new List<ElementId>();
                foreach(GenericForm form in voids)
                {
                    ids.Add(form.Id);
                }

                uidoc.Selection.SetElementIds(ids);

                // Show Result
                if (voids.Count == 0)
                {
                    TaskDialog.Show("Voids", "No voids found in this family");
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
            return typeof(Family_Voids_Select).Namespace + "." + nameof(Family_Voids_Select);
        }
    }
}