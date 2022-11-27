using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_Logic;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.ReadOnly)]
    public class FamilyVoidsSelect : BaseCommand
    {
        private static UIDocument _uidoc;
        private static Document _doc;
        private bool _runFailed;
        private string _runResult;

        public FamilyVoidsSelect()
        {
            _transactionName = "Voids";
        }

        public override Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _result = new RunResult();
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;

            _result.Started = true;

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
                    ShowResult(_result);
                    return Result.Succeeded;
                }

                _uidoc.Selection.SetElementIds(voids.ConvertAll(x => x.Id));

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult(_result);
                return Result.Failed;
            }
        }

        private protected override async void Run(ExternalCommandData commandData) { return; }

        public static string GetPath() => typeof(FamilyVoidsSelect).Namespace + "." + nameof(FamilyVoidsSelect);
    }
}