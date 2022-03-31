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
    public class NamesPrefixChange : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            List<Type> typesAll = new List<Type>() {
                typeof(AreaScheme),
                typeof(BrowserOrganization),
                typeof(BuildingPadType),
                typeof(CeilingType),
                typeof(CurtainSystemType),
                typeof(DimensionType),
                typeof(Family), //typeof(FamilySymbol),
                typeof(FilledRegionType),
                typeof(GridType),
                typeof(GroupType),
                typeof(LevelType),
                typeof(LinePatternElement),
                typeof(Material),
                typeof(PanelType),
                typeof(ContinuousRailType), //typeof(RailingType),
                typeof(FasciaType), //typeof(GutterType), typeof(RoofType),
                typeof(SpotDimensionType),
                typeof(StairsType),
                typeof(StairsLandingType),
                typeof(StairsRunType),
                typeof(TextNoteType),
                typeof(ViewDrafting), //typeof(ViewPlan), typeof(ViewSchedule), typeof(ViewSection),
                typeof(WallType),
                typeof(WallFoundationType)
            };

            int count = 0;

            try
            {
                NamesPrefixChangeForm form = new NamesPrefixChangeForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                NamesPrefixChangeData data = form.DataContext as NamesPrefixChangeData;

                // Getting input data from user
                string inputPrefixOld = data.ResultPrefixOld;
                string inputPrefixNew = data.ResultPrefixNew;
                List<bool> categories = data.ResultCategories;

                List<Type> types = typesAll.Where((name, index) => categories[index]).ToList();

                using (Transaction trans = new Transaction(doc, "Change Names Prefix"))
                {
                    trans.Start();

                    count += ReplaceNames(doc, inputPrefixOld, inputPrefixNew, types);

                    trans.Commit();
                }

                // Show result
                string text = count == 0
                    ? "No prefixes changed"
                    : $"{count} prefixes changed";
                TaskDialog.Show("Names Prefix Change", text);

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
        /// <summary>
        /// Replace substring in names of elements of given types.
        /// <para>
        /// <c>int count = CountPrefixes(doc, "OLD", "NEW", typeof(GutterType));</c>
        /// </para>
        /// </summary>
        /// <param name="doc">Document to process in.</param>
        /// <param name="prefixOld">String to search.</param>
        /// <param name="prefixNew">String to replace with.</param>
        /// <param name="types">Sytem.Type of needed DB classes (categories).</param>
        /// <returns>Count of strings with replaced substrings.</returns>
        private static int ReplaceNames(Document doc, string prefixOld, string prefixNew, List<Type> types)
        {
            int count = 0;

            ElementMulticlassFilter elementMulticlassFilter = new ElementMulticlassFilter(types);

            IEnumerable<Element> elements = new FilteredElementCollector(doc)
                .WherePasses(elementMulticlassFilter)
                .ToElements();
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

        /// <summary>
        /// Gets the full namespace path to this command
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(NamesPrefixChange).Namespace + "." + nameof(NamesPrefixChange);
        }
    }
}
