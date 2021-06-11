using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Family_Zero_Coordinates : IExternalCommand
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

                XYZ o = new XYZ(0, 0, 0);
                XYZ a = new XYZ(length, 0, 0);
                XYZ b = new XYZ(0, length, 0);
                XYZ c = new XYZ(0 - length, 0, 0);
                XYZ d = new XYZ(0, 0 - length, 0);
                
                using (Transaction trans = new Transaction(doc, "Family Zero Coordinates"))
                {
                    trans.Start();

                    Line l_0 = Line.CreateBound(o, a);
                    DetailCurve d_0 = doc.FamilyCreate.NewDetailCurve(view, l_0);
                    Line l_1 = Line.CreateBound(o, b);
                    DetailCurve d_1 = doc.FamilyCreate.NewDetailCurve(view, l_1);
                    Line l_2 = Line.CreateBound(o, c);
                    DetailCurve d_2 = doc.FamilyCreate.NewDetailCurve(view, l_2);
                    Line l_3 = Line.CreateBound(o, d);
                    DetailCurve d_3 = doc.FamilyCreate.NewDetailCurve(view, l_3);

                    List<ElementId> lines = new List<ElementId>();
                    lines.Add(d_0.Id);
                    lines.Add(d_1.Id);
                    lines.Add(d_2.Id);
                    lines.Add(d_3.Id);

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
            return typeof(Family_Zero_Coordinates).Namespace + "." + nameof(Family_Zero_Coordinates);
        }
    }
}