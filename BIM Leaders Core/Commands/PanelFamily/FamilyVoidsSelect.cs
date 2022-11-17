using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.ReadOnly)]
    public class FamilyVoidsSelect : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Voids";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static UIDocument _uidoc;
        private static Document _doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            _runStarted = true;

            try
            {
                List<Type> types = new List<Type> { 
                    typeof(Extrusion),
                    typeof(Blend),
                    typeof(Revolution),
                    typeof(Sweep),
                    typeof(SweptBlend)
                };
                ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

                // Get Geometry primitives
                List<GenericForm> voids = new FilteredElementCollector(_doc)
                    .WherePasses(elementMulticlassFilter)
                    .ToElements()
                    .Cast<GenericForm>()              //LINQ function
                    .Where(x => x.IsSolid == false)   //LINQ function
                    .ToList();                        //LINQ function

                if (voids.Count == 0)
                {
                    _runResult = "No voids found in this family";
                    ShowResult();
                    return Result.Succeeded;
                }

                _uidoc.Selection.SetElementIds(voids.ConvertAll(x => x.Id));

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult();
                return Result.Failed;
            }
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            // ViewModel
            ResultVM formVM = new ResultVM(TRANSACTION_NAME, _runResult);

            // View
            ResultForm form = new ResultForm() { DataContext = formVM };
            form.ShowDialog();
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(FamilyVoidsSelect).Namespace + "." + nameof(FamilyVoidsSelect);
        }
    }
}