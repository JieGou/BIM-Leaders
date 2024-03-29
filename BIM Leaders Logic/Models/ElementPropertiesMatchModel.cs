﻿using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class ElementPropertiesMatchModel : BaseModel
    {
        private int _countPropertiesMatched;

        #region PROPERTIES

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            // Pick first element
            Reference reference0 = Uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            Element elementFrom = Doc.GetElement(reference0.ElementId);

            // Pick second element
            SelectionFilterByCategory category = new SelectionFilterByCategory(elementFrom.Category.Name);

            Reference reference1 = Uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, category);
            Element elementTo = Doc.GetElement(reference1.ElementId);

            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                MatchProperties(elementFrom, elementTo);

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        /// <summary>
        /// Copy all instance properties values from one element to other.
        /// </summary>
        private void MatchProperties(Element elementFrom, Element elementTo)
        {
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
                            _countPropertiesMatched++;
                            break;
                        case StorageType.ElementId:
                            ElementId valueE = parameter.AsElementId();
                            parameterTo.Set(valueE);
                            _countPropertiesMatched++;
                            break;
                        case StorageType.Integer:
                            int valueI = parameter.AsInteger();
                            parameterTo.Set(valueI);
                            _countPropertiesMatched++;
                            break;
                        case StorageType.String:
                            string valueS = parameter.AsString();
                            parameterTo.Set(valueS);
                            _countPropertiesMatched++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Converts parameter set to list of parameters
        /// </summary>
        /// <returns>List of parameters.</returns>
        private List<Parameter> ConvertParameterSet(ParameterSet parametersSet)
        {
            List<Parameter> parametersList = new List<Parameter>();

            ParameterSetIterator iterator = parametersSet.ForwardIterator();
            while (iterator.MoveNext())
            {
                parametersList.Add(iterator.Current as Parameter);
            }

            return parametersList;
        }

        private protected override string GetRunResult()
        {
            string text = (_countPropertiesMatched == 0)
                ? "No properties set."
                : $"{_countPropertiesMatched} properties have been matched.";

            return text;
        }

        #endregion
    }
}