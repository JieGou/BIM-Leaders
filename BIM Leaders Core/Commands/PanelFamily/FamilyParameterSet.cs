﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class FamilyParameterSet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            int count = 0;

            try
            {
                FamilyParameterSetForm form = new FamilyParameterSetForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                FamilyParameterSetData data = form.DataContext as FamilyParameterSetData;
                string parameterName = data.ParametersListSelected;
                string parameterValue = data.ParameterValue;

                // Get parameter
                FamilyParameter parameter = doc.FamilyManager.get_Parameter(parameterName);

                using (Transaction trans = new Transaction(doc, "Set Parameter"))
                {
                    trans.Start();

                    count = ChangeParameter(doc, parameter, parameterValue);

                    trans.Commit();
                }

                // Show result
                string text = (count == 0)
                    ? "No parameters set."
                    : $"{count} parameters set.";
                TaskDialog.Show("Parameter Set", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Chaange the given parameter to given value in all family types.
        /// </summary>
        /// <returns>Count of times when parameter was changed.</returns>
        private static int ChangeParameter(Document doc, FamilyParameter parameter, string parameterValue)
        {
            int count = 0;

            if (parameter.IsReadOnly)
                return count;

            if (parameter.StorageType == StorageType.None)
                return count;

            FamilyTypeSet familyTypeSet = doc.FamilyManager.Types;

            if (parameter.StorageType == StorageType.Integer)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(parameterValue), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(parameterValue), UnitTypeId.Centimeters));
#endif
                    else
                        doc.FamilyManager.Set(parameter, Convert.ToInt32(parameterValue));
                    count++;
                }
                return count;
            }
            if (parameter.StorageType == StorageType.Double)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    doc.FamilyManager.CurrentType = familyType;
#if VERSION2020
                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(parameterValue), DisplayUnitType.DUT_CENTIMETERS));
#else
                    if (parameter.GetUnitTypeId() == UnitTypeId.Centimeters)
                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(parameterValue), UnitTypeId.Centimeters));
#endif
                    else
                        doc.FamilyManager.Set(parameter, Convert.ToDouble(parameterValue));
                    count++;
                }
                return count;
            }
            if (parameter.StorageType == StorageType.String)
            {
                foreach (FamilyType familyType in familyTypeSet)
                {
                    doc.FamilyManager.Set(parameter, parameterValue);
                    count++;
                }
                return count;
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
                        ICollection<ElementId> materialIds = new FilteredElementCollector(doc)
                            .OfClass(typeof(Material))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId materialId in materialIds)
                            if (doc.GetElement(materialId).Name == parameterValue)
                                id = materialId;

                        doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                        count++;
                        break;

                    case ParameterType.FamilyType:
                        ICollection<ElementId> familyTypeIds = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilyType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId familyTypeId in familyTypeIds)
                            if (doc.GetElement(familyTypeId).Name == parameterValue)
                                id = familyTypeId;

                        doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                        count++;
                        break;

                    case ParameterType.Image:
                        ICollection<ElementId> imageIds = new FilteredElementCollector(doc)
                            .OfClass(typeof(ImageType))
                            .WhereElementIsNotElementType()
                            .ToElementIds();
                        foreach (ElementId imageId in imageIds)
                            if (doc.GetElement(imageId).Name == parameterValue)
                                id = imageId;

                        doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
                        count++;
                        break;

                    default:
                        break;
                }
            }
            return count;

        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
        }
    }
}