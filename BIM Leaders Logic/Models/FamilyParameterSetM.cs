using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class FamilyParameterSetM : BaseModel
    {
        private int _countParametersSet = 0;

        #region PROPERTIES

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

        public FamilyParameterSetM(
            ExternalCommandData commandData,
            string transactionName,
            Action<RunResult> showResultAction
            ) : base(commandData, transactionName, showResultAction) { }

        #region METHODS

        private protected override void TryExecute()
        {
            using (Transaction trans = new Transaction(_doc, TransactionName))
            {
                trans.Start();

                ChangeParameter();

                trans.Commit();
            }

            _result.Result = GetRunResult();
        }

        /// <summary>
        /// Change the given parameter to given value in all family types.
        /// Value is given as string, so depends on parameter type value will be converted.
        /// </summary>
        private void ChangeParameter()
        {
            // Get parameter
            FamilyParameter parameter = _doc.FamilyManager.get_Parameter(SelectedParameterName);

            if (parameter.IsReadOnly)
                return;

            if (parameter.StorageType == StorageType.None)
                return;

            FamilyTypeSet familyTypeSet = _doc.FamilyManager.Types;

            if (parameter.StorageType == StorageType.Integer)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(Value), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToInt32(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.Double)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(Value), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToDouble(Value));

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.String)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.Set(parameter, Value);

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.ElementId)
            {
                ElementId id = new ElementId(0);
#if VERSION2023
                switch (parameter.Definition.GetDataType())
                {
                    case ParameterType.Invalid:
                        break;
                    case ParameterType.Text:
                        break;
                    case ParameterType.Material:
                        ICollection<ElementId> materialIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(Material))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId materialId in materialIds)
                            if (_doc.GetElement(materialId).Name == Value)
                                id = materialId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.FamilyType:
                        ICollection<ElementId> familyTypeIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(FamilyType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId familyTypeId in familyTypeIds)
                            if (_doc.GetElement(familyTypeId).Name == Value)
                                id = familyTypeId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.Image:
                        ICollection<ElementId> imageIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId imageId in imageIds)
                            if (_doc.GetElement(imageId).Name == Value)
                                id = imageId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    default:
                        break;
                }
#else
switch (parameter.Definition.ParameterType)
                {
                    case ParameterType.Invalid:
                        break;
                    case ParameterType.Text:
                        break;
                    case ParameterType.Material:
                        ICollection<ElementId> materialIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(Material))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId materialId in materialIds)
                            if (_doc.GetElement(materialId).Name == Value)
                                id = materialId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.FamilyType:
                        ICollection<ElementId> familyTypeIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(FamilyType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId familyTypeId in familyTypeIds)
                            if (_doc.GetElement(familyTypeId).Name == Value)
                                id = familyTypeId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    case ParameterType.Image:
                        ICollection<ElementId> imageIds = new FilteredElementCollector(_doc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId imageId in imageIds)
                            if (_doc.GetElement(imageId).Name == Value)
                                id = imageId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
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