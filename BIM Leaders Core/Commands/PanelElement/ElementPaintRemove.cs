﻿using System;
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

                    (countAll, countCleared) = RemovePaint(doc, element);

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