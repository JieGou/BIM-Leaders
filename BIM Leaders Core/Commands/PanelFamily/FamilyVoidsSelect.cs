using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.ReadOnly)]
    public class FamilyVoidsSelect : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                List<Type> types = new List<Type> { 
                    typeof(Extrusion),
                    typeof(Blend),
                    typeof(Revolution),
                    typeof(Sweep),
                    typeof(SweptBlend)
                };
                ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

                // Get Geometry primitives
                List<GenericForm> voids = new FilteredElementCollector(doc)
                    .WherePasses(elementMulticlassFilter)
                    .ToElements()
                    .Cast<GenericForm>()              //LINQ function
                    .Where(x => x.IsSolid == false)   //LINQ function
                    .ToList();                        //LINQ function

                if (voids.Count == 0)
                {
                    TaskDialog.Show("Voids", "No voids found in this family");
                    return Result.Failed;
                }

                uidoc.Selection.SetElementIds(voids.ConvertAll(x => x.Id));

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