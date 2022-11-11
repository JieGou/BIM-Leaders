using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class ElementPaintRemove : IExternalCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private static int _countFacesAll;
        private static int _countFacesCleared;

        private const string TRANSACTION_NAME = "Remove Paint from Element";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            try
            {
                // Pick Object
                Reference reference = _uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = _doc.GetElement(reference.ElementId);

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    RemovePaint(element);

                    trans.Commit();
                }

                string text = (_countFacesCleared == 0)
                    ? "Painted faces not found."
                    : $"{_countFacesCleared} of {_countFacesAll} faces have been cleared from paint.";

                ShowResult(text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
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

        private static void ShowResult(string resultText)
        {
            if (resultText == null)
                return;

            // ViewModel
            ReportVM formVM = new ReportVM(TRANSACTION_NAME, resultText);

            // View
            ReportForm form = new ReportForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementPaintRemove).Namespace + "." + nameof(ElementPaintRemove);
        }
    }
}