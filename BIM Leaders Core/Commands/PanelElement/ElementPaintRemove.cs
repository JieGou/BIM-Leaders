using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ElementPaintRemove : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            int countAll = 0;
            int countCleared = 0;

            try
            {
                // Pick Object
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = doc.GetElement(reference.ElementId);

                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    RemovePaint(doc, element, ref countAll, ref countCleared);

                    trans.Commit();
                }
                ShowResult(countAll, countCleared);

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
        private static void RemovePaint(Document doc, Element element, ref int count, ref int countCleared)
        {
            Options options = new Options
            {
                ComputeReferences = true
            };

            foreach (Solid solid in element.get_Geometry(options))
            {
                FaceArray faces = solid.Faces;
                count += faces.Size;
                foreach (Face face in faces)
                {
                    if (doc.IsPainted(element.Id, face))
                    {
                        doc.RemovePaint(element.Id, face);
                        countCleared++;
                    }
                }
            }
        }

        private static void ShowResult(int countAll, int countCleared)
        {
            // Show result
            string text = (countCleared == 0)
                ? "Painted faces not found."
                : $"{countCleared} of {countAll} faces have been cleared from paint.";
            
            TaskDialog.Show("Paint remove", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementPaintRemove).Namespace + "." + nameof(ElementPaintRemove);
        }
    }
}