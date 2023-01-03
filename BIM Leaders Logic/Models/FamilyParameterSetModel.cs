using System;
using System.Collections.Generic;
using System.Net;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class FamilyParameterSetModel : BaseModel
    {
        private int _countParametersSet = 0;

        #region PROPERTIES

        public List<string> ParametersList { get; private set; }

        private string _selectedParameterName;
        public string SelectedParameterName
        {
            get { return _selectedParameterName; }
            set
            {
                _selectedParameterName = value;
                OnPropertyChanged(nameof(SelectedParameterName));
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        #endregion

        public FamilyParameterSetModel()
        {
            ParametersList = GetParametersList();
        }

        #region METHODS

        private List<string> GetParametersList()
        {
            // Get unique parameters
            IList<FamilyParameter> parametersNamesAll = Doc.FamilyManager.GetParameters();
            List<FamilyParameter> parameters = new List<FamilyParameter>();
            List<string> parametersNames = new List<string>();
            foreach (FamilyParameter i in parametersNamesAll)
            {
                string parameterName = i.Definition.Name;
                if (!parametersNames.Contains(parameterName))
                {
                    parameters.Add(i);
                    parametersNames.Add(parameterName);
                }
            }
            return parametersNames;
        }

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                ChangeParameter();

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        /// <summary>
        /// Change the given parameter to given value in all family types.
        /// Value is given as string, so depends on parameter type value will be converted.
        /// </summary>
        private void ChangeParameter()
        {
            // Get parameter
            FamilyParameter parameter = Doc.FamilyManager.get_Parameter(SelectedParameterName);

            if (parameter.IsReadOnly)
                return;

            if (parameter.StorageType == StorageType.None)
                return;

            FamilyTypeSet familyTypeSet = Doc.FamilyManager.Types;

            if (parameter.StorageType == StorageType.Integer)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    Doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        Doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        Doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), UnitTypeId.Centimeters));
#endif
                    else
                        Doc.FamilyManager.Set(parameter, Convert.ToInt32(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.Double)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    Doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        Doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        Doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), UnitTypeId.Centimeters));
#endif
                    else
                        Doc.FamilyManager.Set(parameter, Convert.ToDouble(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.String)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    Doc.FamilyManager.Set(parameter, Value);

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.ElementId)
            {
                ElementId id = new ElementId(0);
#if VERSION2023
                ForgeTypeId ft = parameter.Definition.GetDataType();

                if (ft == null)
                    return;

                if (Category.IsBuiltInCategory(ft))
                {
                    ICollection<ElementId> familyTypeIds = new FilteredElementCollector(Doc)
                        .OfClass(typeof(FamilyType))
                        .WhereElementIsNotElementType()
                        .ToElementIds();
                    foreach (ElementId familyTypeId in familyTypeIds)
                        if (Doc.GetElement(familyTypeId).Name == Value)
                            id = familyTypeId;

                    Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                    _countParametersSet++;

                    return;
                }

                if (ft == SpecTypeId.String.Text)
                    return;

                if (ft == SpecTypeId.Reference.Material)
                {
                    ICollection<ElementId> materialIds = new FilteredElementCollector(Doc)
                        .OfClass(typeof(Material))
                        .WhereElementIsNotElementType()
                        .ToElementIds();
                    foreach (ElementId materialId in materialIds)
                        if (Doc.GetElement(materialId).Name == Value)
                            id = materialId;

                    Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                    _countParametersSet++;

                    return;
                }

                if (ft == SpecTypeId.Reference.Image)
                {
                    ICollection<ElementId> imageIds = new FilteredElementCollector(Doc)
                        .OfClass(typeof(ImageType))
                        .WhereElementIsNotElementType()
                        .ToElementIds();
                    foreach (ElementId imageId in imageIds)
                        if (Doc.GetElement(imageId).Name == Value)
                            id = imageId;

                    Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                    _countParametersSet++;

                    return;
                }
                
#else
switch (parameter.Definition.ParameterType)
                {
                    case ParameterType.Invalid:
                        break;
                    case ParameterType.Text:
                        break;
                    case ParameterType.Material:
                        ICollection<ElementId> materialIds = new FilteredElementCollector(Doc)
                            .OfClass(typeof(Material))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId materialId in materialIds)
                            if (Doc.GetElement(materialId).Name == Value)
                                id = materialId;

                        Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.FamilyType:
                        ICollection<ElementId> familyTypeIds = new FilteredElementCollector(Doc)
                            .OfClass(typeof(FamilyType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId familyTypeId in familyTypeIds)
                            if (Doc.GetElement(familyTypeId).Name == Value)
                                id = familyTypeId;

                        Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.Image:
                        ICollection<ElementId> imageIds = new FilteredElementCollector(Doc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId imageId in imageIds)
                            if (Doc.GetElement(imageId).Name == Value)
                                id = imageId;

                        Doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    default:
                        break;
                }
#endif
            }
        }

        private protected override string GetRunResult()
        {
            string text = (_countParametersSet == 0)
                ? "No parameters set."
                : $"{_countParametersSet} parameters set.";

            return text;
        }

#endregion
    }
}