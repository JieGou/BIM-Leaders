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
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int facesCountAll = 0;
            int facesCountCleared = 0;

            try
            {
                // Pick Object
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = doc.GetElement(reference.ElementId);

                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    (facesCountAll, facesCountCleared) = RemovePaint(doc, element);

                    trans.Commit();
                }

                // Show result
                string text = facesCountCleared == 0
                    ? "Painted faces not found."
                    :$"{facesCountCleared} of {facesCountAll} faces have been cleared from paint.";
                TaskDialog.Show("Paint remove", text);

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
        /// <returns>Count of all element faces and faced with removed paint.</returns>
        private static (int, int) RemovePaint(Document doc, Element element)
        {
            int count = 0;
            int countCleared = 0;

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
            return (count, countCleared);
        }
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementPaintRemove).Namespace + "." + nameof(ElementPaintRemove);
        }
    }
}