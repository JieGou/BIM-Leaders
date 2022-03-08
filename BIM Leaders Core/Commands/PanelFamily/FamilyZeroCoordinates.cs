using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class FamilyZeroCoordinates : IExternalCommand
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
                Family family = doc.OwnerFamily;

                double length = 1;

                XYZ zero = new XYZ(0, 0, 0);
                XYZ a = new XYZ(length, 0, 0);
                XYZ b = new XYZ(0, length, 0);
                XYZ c = new XYZ(0 - length, 0, 0);
                XYZ d = new XYZ(0, 0 - length, 0);
                
                using (Transaction trans = new Transaction(doc, "Family Zero Coordinates"))
                {
                    trans.Start();

                    Line line0 = Line.CreateBound(zero, a);
                    DetailCurve curve0 = doc.FamilyCreate.NewDetailCurve(view, line0);
                    Line line1 = Line.CreateBound(zero, b);
                    DetailCurve curve1 = doc.FamilyCreate.NewDetailCurve(view, line1);
                    Line line2 = Line.CreateBound(zero, c);
                    DetailCurve curve2 = doc.FamilyCreate.NewDetailCurve(view, line2);
                    Line line3 = Line.CreateBound(zero, d);
                    DetailCurve curve3 = doc.FamilyCreate.NewDetailCurve(view, line3);

                    List<ElementId> lines = new List<ElementId>
                    {
                        curve0.Id,
                        curve1.Id,
                        curve2.Id,
                        curve3.Id
                    };

                    uidoc.Selection.SetElementIds(lines);

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