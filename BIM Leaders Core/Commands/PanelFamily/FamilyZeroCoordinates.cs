using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyZeroCoordinates : BaseCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static double _linesLength = 1;

        public FamilyZeroCoordinates()
        {
            _transactionName = "Family Zero Coordinates";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            _runStarted = true;

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

                using (Transaction trans = new Transaction(_doc, _transactionName))
                {
                    trans.Start();

                    List<DetailCurve> curves = lines
                        .ConvertAll(x => _doc.FamilyCreate.NewDetailCurve(_doc.ActiveView, x));

                    List<ElementId> curveIds = curves
                        .ConvertAll(x => x.Id);
                    
                    _uidoc.Selection.SetElementIds(curveIds);

                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult();
                return Result.Failed;
            }
        }

        private protected override void Run(ExternalCommandData commandData) { return; }

        public static string GetPath() => typeof(FamilyZeroCoordinates).Namespace + "." + nameof(FamilyZeroCoordinates);
    }
}