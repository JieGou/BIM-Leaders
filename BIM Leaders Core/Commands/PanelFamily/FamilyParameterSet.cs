using System;
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

            try
            {
                // Collector for data provided in window
                FamilyParameterSetData data = new FamilyParameterSetData(uidoc);

                FamilyParameterSetForm form = new FamilyParameterSetForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as FamilyParameterSetData;

                string parameterName = data.ParametersListSelected;
                string parameterValue = data.ParameterValue;

                Family family = doc.OwnerFamily;

                // Get parameter
                //Parameter par = family.LookupParameter(par_name);
                FamilyParameter parameter = doc.FamilyManager.get_Parameter(parameterName);

                using (Transaction trans = new Transaction(doc, "Set Parameter"))
                {
                    trans.Start();

                    int count = 0;
                    if (!parameter.IsReadOnly)
                    {
                        FamilyTypeSet familyTypeSet = doc.FamilyManager.Types;
                        foreach (FamilyType familyType in familyTypeSet)
                        {
                            doc.FamilyManager.CurrentType = familyType;

                            switch (parameter.StorageType)
                            {
                                case StorageType.None:
                                    break;
                                case StorageType.Integer:
                                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(parameterValue), DisplayUnitType.DUT_CENTIMETERS));
                                    else
                                        doc.FamilyManager.Set(parameter, Convert.ToInt32(parameterValue));
                                    count++;
                                    break;
                                case StorageType.Double:
                                    if (parameter.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                                        doc.FamilyManager.Set(parameter, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(parameterValue), DisplayUnitType.DUT_CENTIMETERS));
                                    else
                                        doc.FamilyManager.Set(parameter, Convert.ToDouble(parameterValue));
                                    count++;
                                    break;
                                case StorageType.String:
                                    doc.FamilyManager.Set(parameter, parameterValue);
                                    count++;
                                    break;
                                case StorageType.ElementId:

                                    ElementId id = new ElementId(0);
                                    switch (parameter.Definition.ParameterType)
                                    {
                                        case ParameterType.Invalid:
                                            break;
                                        case ParameterType.Text:
                                            break;
                                        case ParameterType.Material:
                                            FilteredElementCollector collector0 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> materialIds = collector0.OfClass(typeof(Material))
                                                .WhereElementIsNotElementType()
                                                .ToElementIds();
                                            foreach (ElementId materialId in materialIds)
                                                if (doc.GetElement(materialId).Name == parameterValue)
                                                    id = materialId;

                                            doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                                            count++;
                                            break;

                                        case ParameterType.FamilyType:
                                            FilteredElementCollector collector1 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> familyTypeIds = collector1.OfClass(typeof(FamilyType))
                                                .WhereElementIsNotElementType()
                                                .ToElementIds();
                                            foreach (ElementId familyTypeId in familyTypeIds)
                                                if (doc.GetElement(familyTypeId).Name == parameterValue)
                                                    id = familyTypeId;

                                            doc.FamilyManager.Set(parameter, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                                            count++;
                                            break;

                                        case ParameterType.Image:
                                            FilteredElementCollector collector2 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> imageIds = collector2.OfClass(typeof(ImageType))
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
                                    break;

                                default:
                                    break;
                            }
                            // doc.FamilyManager.SetValueString(par, par_value);
                        }
                    }

                    trans.Commit();
                    
                    if (count == 0)
                        TaskDialog.Show("Parameter Set", "No parameters set.");
                    else
                        TaskDialog.Show("Parameter Set", string.Format("{0} parameters set.", count.ToString()));
                }
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
            return typeof(FamilyParameterSet).Namespace + "." + nameof(FamilyParameterSet);
        }
    }
}