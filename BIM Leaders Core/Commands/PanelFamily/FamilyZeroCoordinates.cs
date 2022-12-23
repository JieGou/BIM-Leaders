using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

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
            _result = new RunResult() { Started = true };

            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            try
            {
                XYZ zero = new XYZ(0, 0, 0);

                List<XYZ> points = new List<XYZ>
                {
                    new XYZ(_linesLength, 0, 0),
                    new XYZ(0, _linesLength, 0),
                    new XYZ(0 - _linesLength, 0, 0),
                    new XYZ(0, 0 - _linesLength, 0)
                };

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
                _result.Failed = true;
                _result.Result = e.Message;
                ShowResult(_result);
                return Result.Failed;
            }
        }

        private protected override void Run(ExternalCommandData commandData) { }

        public static string GetPath() => typeof(FamilyZeroCoordinates).Namespace + "." + nameof(FamilyZeroCoordinates);
    }
}