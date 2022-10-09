using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using System.Windows.Controls;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class FamilyParameterSet : IExternalCommand
    {
        private static Document _doc;
        private static int _countParametersSet = 0;
        private static FamilyParameterSetVM _inputData;

        private const string TRANSACTION_NAME = "Set Parameter";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                List<string> parametersList = GetParametersList();

                _inputData = GetUserInput(parametersList);
                if (_inputData == null)
                    return Result.Cancelled;

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    ChangeParameter();

                    trans.Commit();
                }
                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        private static List<string> GetParametersList()
        {
            // Get unique parameters
            IList<FamilyParameter> parametersNamesAll = _doc.FamilyManager.GetParameters();
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

        private static FamilyParameterSetVM GetUserInput(List<string> parametersList)
        {
            FamilyParameterSetForm form = new FamilyParameterSetForm(parametersList);
            form.ShowDialog();

            if (form.DialogResult == false)
                return null;

            // Get user provided information from window
            return form.DataContext as FamilyParameterSetVM;
        }

        /// <summary>
        /// Change the given parameter to given value in all family types.
        /// Value is given as string, so depends on parameter type value will be converted.
        /// </summary>
        private static void ChangeParameter()
        {
            // Get parameter
            FamilyParameter parameter = _doc.FamilyManager.get_Parameter(_inputData.ParametersListSelected);

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
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(_inputData.ParameterValue), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(_inputData.ParameterValue), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToInt32(_inputData.ParameterValue));
                    
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
                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(_inputData.ParameterValue), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        _doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(_inputData.ParameterValue), UnitTypeId.Centimeters));
#endif
                    else
                        _doc.FamilyManager.Set(parameter, Convert.ToDouble(_inputData.ParameterValue));
                    
                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.String)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    _doc.FamilyManager.Set(parameter, _inputData.ParameterValue);

                    _countParametersSet++;
                }
                return;
            }
            if (parameter.StorageType == StorageType.ElementId)
            {
                ElementId id = new ElementId(0);

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
                            if (_doc.GetElement(materialId).Name == _inputData.ParameterValue)
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
                            if (_doc.GetElement(familyTypeId).Name == _inputData.ParameterValue)
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
                            if (_doc.GetElement(imageId).Name == _inputData.ParameterValue)
                                id = imageId;

                        _doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                        _countParametersSet++;
                        break;

                    default:
                        break;
                }
            }
        }

        private static void ShowResult()
        {
            // Show result
            string text = (_countParametersSet == 0)
                ? "No parameters set."
                : $"{_countParametersSet} parameters set.";
            
            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
        }
    }
}