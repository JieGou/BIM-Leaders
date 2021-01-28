using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
/*
import clr
clr.AddReference("ProtoGeometry")
from Autodesk.DesignScript import Geometry as geom

# Import Element wrapper extension methods
clr.AddReference("RevitNodes")
import Revit
from Autodesk.Revit.DB import *

# Import ToProtoType, ToRevitType geometry conversion extension methods
clr.ImportExtensions(Revit.GeometryConversion)

clr.AddReference("RevitAPIUI")
from Autodesk.Revit.UI.Selection import ObjectType

clr.AddReference("ProtoGeometry")
from Autodesk.DesignScript import Geometry as geom
*/

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Stairs_Steps_Enumerate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            UIApplication uiapp = commandData.Application;

            // Get Document
            Autodesk.Revit.DB.Document doc = uidoc.Document;

            // Get View
            View view = doc.ActiveView;

            try
            {
                // Getting input from user
                bool right_side = true;
                double start_number = 1;
                /*
                using (Dimension_Section_Floors_Form form = new Dimension_Section_Floors_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        input_spots = form.Result_Spots();
                        input_thickness_cm = Decimal.ToDouble(form.Result_Thickness());
                    }
                }
                
                double input_thickness = UnitUtils.ConvertToInternalUnits(input_thickness_cm, DisplayUnitType.DUT_CENTIMETERS);
                */
                XYZ zero = new XYZ(0, 0, 0);

                // Get Floors
                FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                IEnumerable<MultistoryStairs> stairs_ms_all = collector.OfCategory(BuiltInCategory.OST_MultistoryStairs)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<MultistoryStairs>();

                // Selecting all levels in the view
                IEnumerable<ElementId> levels_all = collector.OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Selecting all runs in the view
                IEnumerable<StairsRun> runs = collector.OfCategory(BuiltInCategory.OST_StairsRuns)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsRun>();

                // Selecting all stairs in the view
                IEnumerable<Stairs> stairs_all = collector.OfCategory(BuiltInCategory.OST_Stairs)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Stairs>();

                int count = 0;
                int grouped = 0;
                int unpinned = 0;
                
                // Filtering for multistairs that are in groups
                List<MultistoryStairs> stairs_ms = new List<MultistoryStairs>();
                foreach(MultistoryStairs m in stairs_ms_all)
                {
                    if(m.GroupId == ElementId.InvalidElementId)
                    {
                        stairs_ms.Add(m);
                    }
                    else
                    {
                        grouped++;
                    }
                }

                // Filtering for stairs that are in groups
                List<Stairs> stairs_temp = new List<Stairs>();
                foreach (Stairs s in stairs_all)
                {
                    if (s.GroupId == ElementId.InvalidElementId)
                    {
                        stairs_temp.Add(s);
                    }
                    else
                    {
                        grouped++;
                    }
                }

                // Creating list of stairs levels and filtering for Model-In-Place
                List<Stairs> stairs = new List<Stairs>();
                List<double> levels = new List<double>();
                foreach(Stairs stair in stairs_temp)
                {
                    try
                    {
                        levels.Add(stair.BaseElevation);
                        stairs.Add(stair);

                    }
                    catch
                    {

                    }
                }

                // Changing stairs order in a list according to base height
                List<Stairs> stairs_sorted =  stairs.OrderBy(d => levels.IndexOf(d.BaseElevation)).ToList();
                
                // Create annotations
                using (Transaction trans = new Transaction(doc, "Enumerate stairs"))
                {
                    trans.Start();
                    
                    // Unpinning groups (stairs) in multistairs
                    foreach (MultistoryStairs m in stairs_ms)
                    {
                        foreach (ElementId l in levels_all)
                        {
                            try
                            {
                                m.Unpin(l);
                                unpinned++;
                            }
                            catch
                            {

                            }
                        }
                    }
                    /*
                    risers_below = int(IN[0]);
                    side = IN[1]

                    # Changing stairs order in a list according to base height
                    zipped = zip(levels, stairs)
                    stairs = [x for _, x in sorted(zipped)]

                    #  Changing thread numbers
                    for stair in range(len(stairs)):

                        parameter = stairs[stair].get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX)

                        parameter.Set(risers_below)

                        risers_below += stairs[stair].ActualRisersNumber

                    #  Creating thread numbers on the view
                    for run in range(len(runs)):

                        if side:
		                    reference = runs[run].GetNumberSystemReference(StairsNumberSystemReferenceOption.RightQuarter)

                        else:
		                    reference = runs[run].GetNumberSystemReference(StairsNumberSystemReferenceOption.LeftQuarter)

                        run_id = Autodesk.Revit.DB.LinkElementId(runs[run].Id)


                        try:
		                    NumberSystem.Create(doc, view_id, run_id, reference)

                            count += 1

                        except:
                                                    count += 1


                    TransactionManager.Instance.TransactionTaskDone()

                    text = ""
                    if grouped:
	                    text += str(grouped)

                        text += " stairs are in groups! Exclude them from groups! "
                    text += str(count)
                    text += " runs with "
                    text += str(risers_below - 1)
                    text += " treads was numerated. "
                    if unpinned:
	                    text += str(unpinned)

                        text += " stairs was unpinned!"
                    """
                    if pinned:
	                    text += str(pinned)

                        text += " stairs are pinned!"
                    """
                    */

                    trans.Commit();

                    if (count != 0)
                    {
                        TaskDialog.Show("Section Annotations", "No annotations created");
                    }
                    else
                    {
                        if (0 == 0)
                        {
                            TaskDialog.Show("Section Annotations", string.Format("000"));
                        }
                        else
                        {
                            TaskDialog.Show("Section Annotations", string.Format("{0} spot elevations created", count.ToString()));
                        }
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
    }
}
