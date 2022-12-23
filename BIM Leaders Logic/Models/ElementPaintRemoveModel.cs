using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class ElementPaintRemoveModel : BaseModel
    {
        private int _countFacesAll;
        private int _countFacesCleared;

        #region PROPERTIES

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            // Pick Object
            Reference reference = Uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Element element = Doc.GetElement(reference.ElementId);

            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                RemovePaint(element);

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        /// <summary>
        /// Remove paint from all faces of element.
        /// </summary>
        private void RemovePaint(Element element)
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
                    if (Doc.IsPainted(element.Id, face))
                    {
                        Doc.RemovePaint(element.Id, face);
                        _countFacesCleared++;
                    }
                }
            }
        }

        private protected override string GetRunResult()
        {
            string text = (_countFacesCleared == 0)
                ? "Painted faces not found."
                : $"{_countFacesCleared} of {_countFacesAll} faces have been cleared from paint.";

            return text;
        }

        #endregion
    }
}