using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyZeroCoordinates : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Family Zero Coordinates";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static UIDocument _uidoc;
        private static Document _doc;
        private static double _linesLength = 1;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
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

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
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

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ResultVM formVM = new ResultVM(TRANSACTION_NAME, _runResult);

            // View
            ResultForm form = new ResultForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyZeroCoordinates).Namespace + "." + nameof(FamilyZeroCoordinates);
        }
    }
}