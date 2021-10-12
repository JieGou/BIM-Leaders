using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Family_Parameter_Set : IExternalCommand
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
                Family_Parameter_Set_Data data = new Family_Parameter_Set_Data(uidoc);

                Family_Parameter_Set_Form form = new Family_Parameter_Set_Form(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Family_Parameter_Set_Data;

                string par_name = data.par_list_sel;
                string par_value = data.par_value;

                Family family = doc.OwnerFamily;

                // Get parameter
                //Parameter par = family.LookupParameter(par_name);
                FamilyParameter par = doc.FamilyManager.get_Parameter(par_name);

                using (Transaction trans = new Transaction(doc, "Set Parameter"))
                {
                    trans.Start();

                    int count = 0;
                    if (!par.IsReadOnly)
                    {
                        FamilyTypeSet ftp = doc.FamilyManager.Types;
                        foreach (FamilyType f in ftp)
                        {
                            doc.FamilyManager.CurrentType = f;

                            switch (par.StorageType)
                            {
                                case StorageType.None:
                                    break;
                                case StorageType.Integer:
                                    if (par.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                                    {
                                        doc.FamilyManager.Set(par, UnitUtils.ConvertToInternalUnits(Convert.ToInt32(par_value), DisplayUnitType.DUT_CENTIMETERS));
                                    }
                                    else
                                        doc.FamilyManager.Set(par, Convert.ToInt32(par_value));
                                    count++;
                                    break;
                                case StorageType.Double:
                                    if (par.DisplayUnitType == DisplayUnitType.DUT_CENTIMETERS)
                                    {
                                        doc.FamilyManager.Set(par, UnitUtils.ConvertToInternalUnits(Convert.ToDouble(par_value), DisplayUnitType.DUT_CENTIMETERS));
                                    }
                                    else
                                        doc.FamilyManager.Set(par, Convert.ToDouble(par_value));
                                    count++;
                                    break;
                                case StorageType.String:
                                    doc.FamilyManager.Set(par, par_value);
                                    count++;
                                    break;
                                case StorageType.ElementId:

                                    ElementId id = new ElementId(0);
                                    switch (par.Definition.ParameterType)
                                    {
                                        case ParameterType.Invalid:
                                            break;
                                        case ParameterType.Text:
                                            break;
                                        case ParameterType.Material:
                                            FilteredElementCollector collector_0 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> materials = collector_0.OfClass(typeof(Material))
                                                .WhereElementIsNotElementType()
                                                .ToElementIds();
                                            foreach (ElementId i in materials)
                                                if (doc.GetElement(i).Name == par_value)
                                                    id = i;

                                            doc.FamilyManager.Set(par, id); // NEED TO ADD ERROR IF MATERIAL WITH GIVEN NAME NOT FOUND !!!
                                            count++;
                                            break;

                                        case ParameterType.FamilyType:
                                            FilteredElementCollector collector_1 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> family_types = collector_1.OfClass(typeof(FamilyType))
                                                .WhereElementIsNotElementType()
                                                .ToElementIds();
                                            foreach (ElementId i in family_types)
                                                if (doc.GetElement(i).Name == par_value)
                                                    id = i;

                                            doc.FamilyManager.Set(par, id); // NEED TO ADD ERROR IF FAMILY TYPE WITH GIVEN NAME NOT FOUND !!!
                                            count++;
                                            break;

                                        case ParameterType.Image:
                                            FilteredElementCollector collector_2 = new FilteredElementCollector(doc);
                                            ICollection<ElementId> images = collector_2.OfClass(typeof(ImageType))
                                                .WhereElementIsNotElementType()
                                                .ToElementIds();
                                            foreach (ElementId i in images)
                                                if (doc.GetElement(i).Name == par_value)
                                                    id = i;

                                            doc.FamilyManager.Set(par, id); // NEED TO ADD ERROR IF IMAGE WITH GIVEN NAME NOT FOUND !!!
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
                    {
                        TaskDialog.Show("Parameter Set", "No parameters set.");
                    }
                    else
                    {
                        TaskDialog.Show("Parameter Set", string.Format("{0} parameters set.", count.ToString()));
                    }
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
            return typeof(Family_Parameter_Set).Namespace + "." + nameof(Family_Parameter_Set);
        }
    }
}