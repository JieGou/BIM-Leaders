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

namespace BIM_Leaders_Core
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

                using (Stairs_Steps_Enumerate_Form form = new Stairs_Steps_Enumerate_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        right_side = form.Result_Side();
                        start_number = Decimal.ToDouble(form.Result_Number());
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }

                // Get Floors
                IEnumerable<MultistoryStairs> stairs_ms_all = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_MultistoryStairs)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<MultistoryStairs>();

                // Selecting all levels in the view
                IEnumerable<ElementId> levels_all = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Levels)
                    .WhereElementIsNotElementType()
                    .ToElementIds();

                // Selecting all runs in the view
                IEnumerable<StairsRun> runs = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_StairsRuns)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<StairsRun>();

                // Selecting all stairs in the view
                IEnumerable<Stairs> stairs_all = new FilteredElementCollector(doc, view.Id).OfCategory(BuiltInCategory.OST_Stairs)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .Cast<Stairs>();

                int count = 0;
                int grouped = 0;
                int unpinned = 0;

                // Filtering for multistairs that are in groups
                List<MultistoryStairs> stairs_ms = new List<MultistoryStairs>();
                foreach (MultistoryStairs m in stairs_ms_all)
                {
                    if (m.GroupId == ElementId.InvalidElementId)
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
                foreach (Stairs stair in stairs_temp)
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
                IOrderedEnumerable<Stairs> stairs_sorted = stairs.OrderBy(d => d.BaseElevation);

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

                    // Changing thread numbers
                    foreach (Stairs s in stairs_sorted)
                    {
                        Parameter p = s.get_Parameter(BuiltInParameter.STAIRS_TRISER_NUMBER_BASE_INDEX);
                        p.Set(start_number);
                        start_number += s.ActualRisersNumber;
                    }

                    // Creating thread numbers on the view
                    foreach (StairsRun r in runs)
                    {
                        Reference refer = r.GetNumberSystemReference(StairsNumberSystemReferenceOption.LeftQuarter);

                        if (right_side)
                        {
                            refer = r.GetNumberSystemReference(StairsNumberSystemReferenceOption.RightQuarter);
                        }

                        LinkElementId run_id = new LinkElementId(r.Id);

                        try
                        {
                            NumberSystem.Create(doc, view.Id, run_id, refer);
                            count++;
                        }
                        catch
                        {
                            count++;
                        }
                    }

                    trans.Commit();

                    string text = "";
                    if (grouped > 0)
                    {
                        text += grouped.ToString();
                        text += " stairs are in groups! Exclude them from groups!";
                    }
                    text += count.ToString();
                    text += " runs with ";
                    text += (start_number - 1).ToString();
                    text += " threads was numbered. ";
                    if (unpinned > 0)
                    {
                        text += unpinned.ToString();
                        text += " stairs was unpinned!";
                    }
                    /*
                    if(pinned > 0)
                    {
                        text += pinned.ToString();
                        text += " stairs are pinned!";
                    }
                    */

                    if (count == 0)
                    {
                        TaskDialog.Show("Section Annotations", "No annotations created");
                    }
                    else
                    {
                        TaskDialog.Show("Section Annotations", text);
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
            return typeof(Stairs_Steps_Enumerate).Namespace + "." + nameof(Stairs_Steps_Enumerate);
        }
    }
}