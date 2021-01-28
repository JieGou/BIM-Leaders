using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
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

                Options options = new Options();
                options.ComputeReferences = true;

                int faces_all = 0;
                int faces_cleared = 0;
                using (Transaction trans = new Transaction(doc, "Remove Paint from Element"))
                {
                    trans.Start();

                    foreach (Solid solid in element.get_Geometry(options))
                    {
                        FaceArray faces = solid.Faces;
                        faces_all = faces.Size;
                        foreach (Face face in faces)
                        {
                            if (doc.IsPainted(element.Id, face))
                            {
                                doc.RemovePaint(element.Id, face);
                                faces_cleared++;
                            }
                        }
                    }

                    trans.Commit();
                }  

                // Show result
                if (faces_cleared == 0)
                {
                    TaskDialog.Show("Paint remove", "Painted faces not found");
                }
                else
                {
                    TaskDialog.Show("Paint remove", string.Format("{0} of {1} faces have been cleared from paint", faces_cleared.ToString(), faces_all.ToString()));
                }

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}