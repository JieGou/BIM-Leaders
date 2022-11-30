using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ElementPaintRemove : BaseCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static int _countFacesAll;
        private static int _countFacesCleared;

        public ElementPaintRemove()
        {
            _transactionName = "Remove Paint from Element";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult() { Started = true };

            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            try
            {
                // Pick Object
                Reference reference = _uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = _doc.GetElement(reference.ElementId);

                using (Transaction trans = new Transaction(_doc, _transactionName))
                {
                    trans.Start();

                    RemovePaint(element);

                    trans.Commit();
                }

                _result.Result = GetRunResult();

                ShowResult(_result);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _result.Failed = true;
                _result.Result = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Remove paint from all faces of element.
        /// </summary>
        private static void RemovePaint(Element element)
        {
            Options options = new Options
            {
                ComputeReferences = true
            };

            foreach (Solid solid in element.get_Geometry(options))
            {
                FaceArray faces = solid.Faces;
                _countFacesAll += faces.Size;
                foreach (Face face in faces)
                {
                    if (_doc.IsPainted(element.Id, face))
                    {
                        _doc.RemovePaint(element.Id, face);
                        _countFacesCleared++;
                    }
                }
            }
        }

        private string GetRunResult()
        {
            string text = (_countFacesCleared == 0)
                ? "Painted faces not found."
                : $"{_countFacesCleared} of {_countFacesAll} faces have been cleared from paint.";

            return text;
        }

        private protected override void Run(ExternalCommandData commandData) { return; }

        public static string GetPath() => typeof(ElementPaintRemove).Namespace + "." + nameof(ElementPaintRemove);
    }
}