﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class FamilyParameterChange : IExternalCommand
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
                /*
                FamilyParameterSetForm form = new FamilyParameterSetForm(uidoc);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                FamilyParameterSetData data = form.DataContext as FamilyParameterSetData;
                string parameterName = data.ParametersListSelected;
                string parameterValue = data.ParameterValue;
                */
                // Get parameter
                //FamilyParameter parameter = doc.FamilyManager.get_Parameter(parameterName);

                using (Transaction trans = new Transaction(doc, "Change Parameter"))
                {
                    trans.Start();

                    count = SwitchParameter(doc);

                    trans.Commit();
                }

                // Show result
                string text = (count == 0)
                    ? "No parameters changed."
                    : $"{count} parameters changed.";
                TaskDialog.Show("Parameter Change", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Chaange the given parameter from shared to family parameter.
        /// </summary>
        /// <returns>Count of parameters changed.</returns>
        private static int SwitchParameter(Document doc)
        {
            int count = 0;

            IEnumerable<FamilyParameter> parameters = doc.FamilyManager.GetParameters().Where(x => x.IsShared);
            foreach (FamilyParameter parameter in parameters)
            {
                string parameterName = parameter.Definition.Name;
                string parameterNameTemp = parameterName + "_";
                BuiltInParameterGroup parameterGroup = parameter.Definition.ParameterGroup;
                bool parameterIsInstance = parameter.IsInstance;

                FamilyParameter parameterNew = doc.FamilyManager.ReplaceParameter(parameter, parameterNameTemp, parameterGroup, parameterIsInstance);
                doc.FamilyManager.RenameParameter(parameterNew, parameterName);
                count++;
            }
            count = parameters.Count();
            return count;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyParameterChange).Namespace + "." + nameof(FamilyParameterChange);
        }
    }
}