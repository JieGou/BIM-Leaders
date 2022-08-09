using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class FamilyZeroCoordinates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            double length = 1;

            try
            {
                XYZ zero = new XYZ(0, 0, 0);

                List<XYZ> points = new List<XYZ>();
                points.Add(new XYZ(length, 0, 0));
                points.Add(new XYZ(0, length, 0));
                points.Add(new XYZ(0 - length, 0, 0));
                points.Add(new XYZ(0, 0 - length, 0));

                List<Line> lines = points
                    .ConvertAll(x => Line.CreateBound(zero, x));

                using (Transaction trans = new Transaction(doc, "Family Zero Coordinates"))
                {
                    trans.Start();

                    List<DetailCurve> curves = lines
                        .ConvertAll(x => doc.FamilyCreate.NewDetailCurve(doc.ActiveView, x));

                    List<ElementId> curveIds = curves
                        .ConvertAll(x => x.Id);
                    
                    uidoc.Selection.SetElementIds(curveIds);

                    trans.Commit();
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
            return typeof(FamilyZeroCoordinates).Namespace + "." + nameof(FamilyZeroCoordinates);
        }
    }
}