using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class FamilyVoidsSelect : IExternalCommand
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

                // Get Geometry primitives
                IList<Element> formExt = new FilteredElementCollector(doc).OfClass(typeof(Extrusion)).ToElements();
                IList<Element> formBln = new FilteredElementCollector(doc).OfClass(typeof(Blend)).ToElements();
                IList<Element> formRev = new FilteredElementCollector(doc).OfClass(typeof(Revolution)).ToElements();
                IList<Element> formSwp = new FilteredElementCollector(doc).OfClass(typeof(Sweep)).ToElements();
                IList<Element> formSwb = new FilteredElementCollector(doc).OfClass(typeof(SweptBlend)).ToElements();

                // Find Voids
                List<Element> voids = new List<Element>();
                foreach (GenericForm form in formExt)
                    if(!form.IsSolid)
                        voids.Add(form);
                foreach (GenericForm form in formBln)
                    if (!form.IsSolid)
                        voids.Add(form);
                foreach (GenericForm form in formRev)
                    if (!form.IsSolid)
                        voids.Add(form);
                foreach (GenericForm form in formSwp)
                    if (!form.IsSolid)
                        voids.Add(form);
                foreach (GenericForm form in formSwb)
                    if (!form.IsSolid)
                        voids.Add(form);

                // Selection
                List<ElementId> voidIds = new List<ElementId>();
                foreach(GenericForm form in voids)
                    voidIds.Add(form.Id);

                uidoc.Selection.SetElementIds(voidIds);

                // Show Result
                if (voids.Count == 0)
                    TaskDialog.Show("Voids", "No voids found in this family");

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
            return typeof(FamilyVoidsSelect).Namespace + "." + nameof(FamilyVoidsSelect);
        }
    }
}