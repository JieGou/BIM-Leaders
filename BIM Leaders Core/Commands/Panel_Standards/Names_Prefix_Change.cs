using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Names_Prefix_Change : IExternalCommand
    {
        /// <summary>
        /// Replace substring in names of elements of given category.
        /// <para>
        /// <c>int count = CountPrefixes(doc, "OLD", "NEW", typeof(GutterType));</c>
        /// </para>
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <param name="prefixOld">String to search.</param>
        /// <param name="prefixNew">String to replace with.</param>
        /// <param name="type">Sytem.Type of needed DB class (category).</param>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNames(Document doc, string prefixOld, string prefixNew, Type type)
        {
            int count = 0;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IEnumerable<Element> elements = collector.OfClass(type).ToElements();
            foreach (Element element in elements)
            {
                string name = element.Name;
                if (name.Contains(prefixOld))
                {
                    string nameNew = name.Replace(prefixOld, prefixNew);
                    element.Name = nameNew;
                    count++;
                }
            }

            return count;
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                // Collector for data provided in window
                Names_Prefix_Change_Data data = new Names_Prefix_Change_Data();

                Names_Prefix_Change_Form form = new Names_Prefix_Change_Form();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                data = form.DataContext as Names_Prefix_Change_Data;

                // Getting input data from user
                string inputPrefixOld = data.result_prefix_old;
                string inputPrefixNew = data.result_prefix_new;
                List<bool> categories = data.result_categories;
                int count = 0;

                using (Transaction trans = new Transaction(doc, "Change Names Prefix"))
                {
                    trans.Start();

                    if (categories[0])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(AreaScheme));
                    if (categories[1])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(BrowserOrganization));
                    if (categories[2])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(BuildingPadType));
                    if (categories[3])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(CeilingType));
                    if (categories[4])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(CurtainSystemType));
                    if (categories[5])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(DimensionType));
                    if (categories[6])
                    {
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(Family));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(FamilySymbol));
                    }
                    if (categories[7])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(FilledRegionType));
                    if (categories[8])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(GridType));
                    if (categories[9])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(GroupType));
                    if (categories[10])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(LevelType));
                    if (categories[11])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(LinePatternElement));
                    if (categories[12])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(Material));
                    if (categories[13])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(PanelType));
                    if (categories[14])
                    {
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(ContinuousRailType));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(RailingType));
                    }
                    if (categories[15])
                    {
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(FasciaType));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(GutterType));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(RoofType));
                    }
                    if (categories[16])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(SpotDimensionType));
                    if (categories[17])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(StairsType));
                    if (categories[18])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(StairsLandingType));
                    if (categories[19])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(StairsRunType));
                    if (categories[20])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(TextNoteType));
                    if (categories[21])
                    {
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(ViewDrafting));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(ViewPlan));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(ViewSchedule));
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(ViewSection));
                    }
                    if (categories[22])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(WallType));
                    if (categories[23])
                        count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, typeof(WallFoundationType));

                    trans.Commit();

                    if (count == 0)
                        TaskDialog.Show("Names Prefix Change", "No prefixes changed");
                    else
                        TaskDialog.Show("Names Prefix Change", string.Format("{0} prefixes changed", count.ToString()));
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        /// <summary>
        /// Gets the full namespace path to this command
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(Names_Prefix_Change).Namespace + "." + nameof(Names_Prefix_Change);
        }
    }
}
