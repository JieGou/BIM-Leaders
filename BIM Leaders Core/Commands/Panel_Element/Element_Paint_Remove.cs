using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Element_Paint_Remove : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                // Pick Object
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = doc.GetElement(reference.ElementId);

                Options options = new Options
                {
                    ComputeReferences = true
                };

                int facesCountAll = 0;
                int facesCountCleared = 0;
                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    foreach (Solid solid in element.get_Geometry(options))
                    {
                        FaceArray faces = solid.Faces;
                        facesCountAll = faces.Size;
                        foreach (Face face in faces)
                        {
                            if (doc.IsPainted(element.Id, face))
                            {
                                doc.RemovePaint(element.Id, face);
                                facesCountCleared++;
                            }
                        }
                    }

                    trans.Commit();
                }  

                // Show result
                if (facesCountCleared == 0)
                    TaskDialog.Show("Paint remove", "Painted faces not found");
                else
                    TaskDialog.Show("Paint remove", string.Format("{0} of {1} faces have been cleared from paint", facesCountCleared.ToString(), facesCountAll.ToString()));

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
            return typeof(Element_Paint_Remove).Namespace + "." + nameof(Element_Paint_Remove);
        }
    }
}