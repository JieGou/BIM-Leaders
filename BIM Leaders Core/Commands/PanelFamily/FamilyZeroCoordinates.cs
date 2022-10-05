using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyZeroCoordinates : IExternalCommand
    {
        private static double _linesLength = 1;

        private const string TRANSACTION_NAME = "Family Zero Coordinates"; 

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                XYZ zero = new XYZ(0, 0, 0);

                List<XYZ> points = new List<XYZ>();
                points.Add(new XYZ(_linesLength, 0, 0));
                points.Add(new XYZ(0, _linesLength, 0));
                points.Add(new XYZ(0 - _linesLength, 0, 0));
                points.Add(new XYZ(0, 0 - _linesLength, 0));

                List<Line> lines = points
                    .ConvertAll(x => Line.CreateBound(zero, x));

                using (Transaction trans = new Transaction(doc, TRANSACTION_NAME))
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