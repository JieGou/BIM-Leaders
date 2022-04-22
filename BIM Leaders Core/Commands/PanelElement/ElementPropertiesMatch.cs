using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ElementPropertiesMatch : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int countProperties = 0;

            try
            {
                // Pick first element
                Reference reference0 = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element elementFrom = doc.GetElement(reference0.ElementId);

                // Pick second element
                SelectionFilterByCategory category = new SelectionFilterByCategory(elementFrom.Category.Name);

                Reference reference1 = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, category);
                Element elementTo = doc.GetElement(reference1.ElementId);

                using (Transaction trans = new Transaction(doc, "Match instance properties"))
                {
                    trans.Start();

                    countProperties = MatchProperties(elementFrom, elementTo);

                    trans.Commit();
                }

                // Show result
                string text = (countProperties == 0)
                    ? "No properties set."
                    : $"{countProperties} properties have been matched.";
                TaskDialog.Show("Match instance properties", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Copy all instance properties values from one element to other.
        /// </summary>
        /// <returns>Count of all element properties changed.</returns>
        private static int MatchProperties(Element elementFrom, Element elementTo)
        {
            int count = 0;

            List<Parameter> parametersFrom = ConvertParameterSet(elementFrom.Parameters);
            List<Parameter> parametersTo = ConvertParameterSet(elementTo.Parameters);

            List<ElementId> parametersToIds = parametersTo.ConvertAll(x => x.Id);

            // List of parameter that not to change.
            List<string> parametersExclude = new List<string>
            {
                "Family",
                "Family and Type",
                "Image",
                "Type"
            };

            foreach (Parameter parameter in parametersFrom)
            {
                if (parametersExclude.Contains(parameter.Definition.Name) || parameter.IsReadOnly)
                    continue;

                if (parametersToIds.Contains(parameter.Id))
                {
                    Parameter parameterTo = parametersTo.ElementAt(parametersToIds.IndexOf(parameter.Id));

                    switch (parameter.StorageType)
                    {
                        case StorageType.Double:
                            double valueD = parameter.AsDouble();
                            parameterTo.Set(valueD);
                            count++;
                            break;
                        case StorageType.ElementId:
                            ElementId valueE = parameter.AsElementId();
                            parameterTo.Set(valueE);
                            count++;
                            break;
                        case StorageType.Integer:
                            int valueI = parameter.AsInteger();
                            parameterTo.Set(valueI);
                            count++;
                            break;
                        case StorageType.String:
                            string valueS = parameter.AsString();
                            parameterTo.Set(valueS);
                            count++;
                            break;
                        default:
                            break;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Converts parameter set to list of parameters
        /// </summary>
        /// <returns>List of parameters.</returns>
        private static List<Parameter> ConvertParameterSet(ParameterSet parametersSet)
        {
            List<Parameter> parametersList = new List<Parameter>();

            ParameterSetIterator iterator = parametersSet.ForwardIterator();
            while (iterator.MoveNext())
            {
                parametersList.Add(iterator.Current as Parameter);
            }

            return parametersList;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ElementPropertiesMatch).Namespace + "." + nameof(ElementPropertiesMatch);
        }
    }
}