using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Names_Prefix_Change : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                // Getting input data from user
                string prefix_old = "OLD";
                string prefix_new = "NEW";
                List<bool> categories = Enumerable.Repeat(false, 24).ToList();

                using (Names_Prefix_Change_Form form = new Names_Prefix_Change_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        prefix_old = form.Result_prefix_old();
                        prefix_new = form.Result_prefix_new();
                        categories = form.Result_categories();
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }

                int count = 0;

                using (Transaction trans = new Transaction(doc, "Change Names Prefix"))
                {
                    trans.Start();

                    if (categories[0])
                    {
                        FilteredElementCollector collector_area_schemes = new FilteredElementCollector(doc);
                        IEnumerable<AreaScheme> area_schemes = collector_area_schemes.OfClass(typeof(AreaScheme))
                            .ToElements().Cast<AreaScheme>();
                        foreach (AreaScheme ass in area_schemes)
                        {
                            string name = ass.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                ass.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[1])
                    {
                        FilteredElementCollector collector_browser_organization = new FilteredElementCollector(doc);
                        IEnumerable<BrowserOrganization> browser_organization = collector_browser_organization.OfClass(typeof(BrowserOrganization))
                            .ToElements().Cast<BrowserOrganization>();
                        foreach (BrowserOrganization bo in browser_organization)
                        {
                            string name = bo.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                bo.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[2])
                    {
                        FilteredElementCollector collector_building_pad_types = new FilteredElementCollector(doc);
                        IEnumerable<BuildingPadType> building_pad_types = collector_building_pad_types.OfClass(typeof(BuildingPadType))
                            .ToElements().Cast<BuildingPadType>();
                        foreach (BuildingPadType bpt in building_pad_types)
                        {
                            string name = bpt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                bpt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[3])
                    {
                        FilteredElementCollector collector_ceiling_types = new FilteredElementCollector(doc);
                        IEnumerable<CeilingType> ceiling_types = collector_ceiling_types.OfClass(typeof(CeilingType))
                            .ToElements().Cast<CeilingType>();
                        foreach (CeilingType ct in ceiling_types)
                        {
                            string name = ct.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                ct.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[4])
                    {
                        FilteredElementCollector collector_curtain_system_types = new FilteredElementCollector(doc);
                        IEnumerable<CurtainSystemType> curtain_system_types = collector_curtain_system_types.OfClass(typeof(CurtainSystemType))
                            .ToElements().Cast<CurtainSystemType>();
                        foreach (CurtainSystemType cst in curtain_system_types)
                        {
                            string name = cst.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                cst.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[5])
                    {
                        FilteredElementCollector collector_dimension_types = new FilteredElementCollector(doc);
                        IEnumerable<DimensionType> dimension_types = collector_dimension_types.OfClass(typeof(DimensionType))
                            .ToElements().Cast<DimensionType>();
                        foreach (DimensionType dt in dimension_types)
                        {
                            string name = dt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                dt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[6])
                    {
                        FilteredElementCollector collector_families = new FilteredElementCollector(doc);
                        IEnumerable<Family> families = collector_families.OfClass(typeof(Family))
                            .ToElements().Cast<Family>();
                        FilteredElementCollector collector_family_symbols = new FilteredElementCollector(doc);
                        IEnumerable<FamilySymbol> family_symbols = collector_family_symbols.OfClass(typeof(FamilySymbol))
                            .ToElements().Cast<FamilySymbol>();
                        foreach (Family f in families)
                        {
                            string name = f.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                f.Name = name_new;
                                count++;
                            }
                        }
                        foreach (FamilySymbol fs in family_symbols)
                        {
                            string name = fs.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                fs.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[7])
                    {
                        FilteredElementCollector collector_filled_region_types = new FilteredElementCollector(doc);
                        IEnumerable<FilledRegionType> filled_region_types = collector_filled_region_types.OfClass(typeof(FilledRegionType))
                            .ToElements().Cast<FilledRegionType>();
                        foreach (FilledRegionType frt in filled_region_types)
                        {
                            string name = frt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                frt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[8])
                    {
                        FilteredElementCollector collector_grid_types = new FilteredElementCollector(doc);
                        IEnumerable<GridType> grid_types = collector_grid_types.OfClass(typeof(GridType))
                            .ToElements().Cast<GridType>();
                        foreach (GridType gdt in grid_types)
                        {
                            string name = gdt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                gdt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[9])
                    {
                        FilteredElementCollector collector_group_types = new FilteredElementCollector(doc);
                        IEnumerable<GroupType> group_types = collector_group_types.OfClass(typeof(GroupType))
                            .ToElements().Cast<GroupType>();
                        foreach (GroupType gt in group_types)
                        {
                            string name = gt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                gt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[10])
                    {
                        FilteredElementCollector collector_level_types = new FilteredElementCollector(doc);
                        IEnumerable<LevelType> level_types = collector_level_types.OfClass(typeof(LevelType))
                            .ToElements().Cast<LevelType>();
                        foreach (LevelType lt in level_types)
                        {
                            string name = lt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                lt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[11])
                    {
                        FilteredElementCollector collector_line_patterns = new FilteredElementCollector(doc);
                        IEnumerable<LinePatternElement> line_patterns = collector_line_patterns.OfClass(typeof(LinePatternElement))
                            .ToElements().Cast<LinePatternElement>();
                        foreach (LinePatternElement lp in line_patterns)
                        {
                            string name = lp.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                lp.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[12])
                    {
                        FilteredElementCollector collector_materials = new FilteredElementCollector(doc);
                        IEnumerable<Material> materials = collector_materials.OfClass(typeof(Material))
                            .ToElements().Cast<Material>();
                        foreach (Material m in materials)
                        {
                            string name = m.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                m.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[13])
                    {
                        FilteredElementCollector collector_panel_types = new FilteredElementCollector(doc);
                        IEnumerable<PanelType> panel_types = collector_panel_types.OfClass(typeof(PanelType))
                            .ToElements().Cast<PanelType>();
                        foreach (PanelType pt in panel_types)
                        {
                            string name = pt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                pt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[14])
                    {
                        FilteredElementCollector collector_continuous_rail_types = new FilteredElementCollector(doc);
                        IEnumerable<ContinuousRailType> continuous_rail_types = collector_continuous_rail_types.OfClass(typeof(ContinuousRailType))
                            .Cast<ContinuousRailType>();
                        FilteredElementCollector collector_railing_types = new FilteredElementCollector(doc);
                        IEnumerable<RailingType> railing_types = collector_railing_types.OfClass(typeof(RailingType))
                            .ToElements().Cast<RailingType>();
                        foreach (ContinuousRailType crt in continuous_rail_types)
                        {
                            string name = crt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                crt.Name = name_new;
                                count++;
                            }
                        }
                        foreach (RailingType rt in railing_types)
                        {
                            string name = rt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                rt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[15])
                    {
                        FilteredElementCollector collector_fascia_types = new FilteredElementCollector(doc);
                        IEnumerable<FasciaType> fascia_types = collector_fascia_types.OfClass(typeof(FasciaType))
                            .ToElements().Cast<FasciaType>();
                        FilteredElementCollector collector_gutter_types = new FilteredElementCollector(doc);
                        IEnumerable<GutterType> gutter_types = collector_gutter_types.OfClass(typeof(GutterType))
                            .ToElements().Cast<GutterType>();
                        FilteredElementCollector collector_roof_types = new FilteredElementCollector(doc);
                        IEnumerable<RoofType> roof_types = collector_roof_types.OfClass(typeof(RoofType))
                            .ToElements().Cast<RoofType>();
                        foreach (FasciaType ft in fascia_types)
                        {
                            string name = ft.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                ft.Name = name_new;
                                count++;
                            }
                        }
                        foreach (GutterType gt in gutter_types)
                        {
                            string name = gt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                gt.Name = name_new;
                                count++;
                            }
                        }
                        foreach (RoofType rft in roof_types)
                        {
                            string name = rft.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                rft.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[16])
                    {
                        FilteredElementCollector collector_spot_dimension_types = new FilteredElementCollector(doc);
                        IEnumerable<SpotDimensionType> spot_dimension_types = collector_spot_dimension_types.OfClass(typeof(SpotDimensionType))
                            .ToElements().Cast<SpotDimensionType>();
                        foreach (SpotDimensionType sdt in spot_dimension_types)
                        {
                            string name = sdt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                sdt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[17])
                    {
                        FilteredElementCollector collector_stair_types = new FilteredElementCollector(doc);
                        IEnumerable<StairsType> stair_types = collector_stair_types.OfClass(typeof(StairsType))
                            .ToElements().Cast<StairsType>();
                        foreach (StairsType st in stair_types)
                        {
                            string name = st.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                st.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[18])
                    {
                        FilteredElementCollector collector_stair_landing_types = new FilteredElementCollector(doc);
                        IEnumerable<StairsLandingType> stair_landing_types = collector_stair_landing_types.OfClass(typeof(StairsLandingType))
                            .ToElements().Cast<StairsLandingType>();
                        foreach (StairsLandingType slt in stair_landing_types)
                        {
                            string name = slt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                slt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[19])
                    {
                        FilteredElementCollector collector_stair_run_types = new FilteredElementCollector(doc);
                        IEnumerable<StairsRunType> stair_run_types = collector_stair_run_types.OfClass(typeof(StairsRunType))
                            .ToElements().Cast<StairsRunType>();
                        foreach (StairsRunType srt in stair_run_types)
                        {
                            string name = srt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                srt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[20])
                    {
                        FilteredElementCollector collector_text_note_types = new FilteredElementCollector(doc);
                        IEnumerable<TextNoteType> text_note_types = collector_text_note_types.OfClass(typeof(TextNoteType))
                            .ToElements().Cast<TextNoteType>();
                        foreach (TextNoteType tnt in text_note_types)
                        {
                            string name = tnt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                tnt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[21])
                    {
                        FilteredElementCollector collector_views_drafting = new FilteredElementCollector(doc);
                        IEnumerable<ViewDrafting> views_drafting = collector_views_drafting.OfClass(typeof(ViewDrafting))
                            .ToElements().Cast<ViewDrafting>();
                        FilteredElementCollector collector_views_plan = new FilteredElementCollector(doc);
                        IEnumerable<ViewPlan> views_plan = collector_views_plan.OfClass(typeof(ViewPlan))
                            .ToElements().Cast<ViewPlan>();
                        FilteredElementCollector collector_views_schedule = new FilteredElementCollector(doc);
                        IEnumerable<ViewSchedule> views_schedule = collector_views_schedule.OfClass(typeof(ViewSchedule))
                            .ToElements().Cast<ViewSchedule>();
                        FilteredElementCollector collector_views_section = new FilteredElementCollector(doc);
                        IEnumerable<ViewSection> views_section = collector_views_section.OfClass(typeof(ViewSection))
                            .ToElements().Cast<ViewSection>();
                        foreach (ViewDrafting vd in views_drafting)
                        {
                            string name = vd.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                vd.Name = name_new;
                                count++;
                            }
                        }
                        foreach (ViewPlan vp in views_plan)
                        {
                            string name = vp.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                vp.Name = name_new;
                                count++;
                            }
                        }
                        foreach (ViewSchedule vs in views_schedule)
                        {
                            string name = vs.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                vs.Name = name_new;
                                count++;
                            }
                        }
                        foreach (ViewSection vsn in views_section)
                        {
                            string name = vsn.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                vsn.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[22])
                    {
                        FilteredElementCollector collector_wall_types = new FilteredElementCollector(doc);
                        IEnumerable<WallType> wall_types = collector_wall_types.OfClass(typeof(WallType))
                            .ToElements().Cast<WallType>();
                        foreach (WallType wt in wall_types)
                        {
                            string name = wt.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                wt.Name = name_new;
                                count++;
                            }
                        }
                    }

                    if (categories[23])
                    {
                        FilteredElementCollector collector_wall_foundation_types = new FilteredElementCollector(doc);
                        IEnumerable<WallFoundationType> wall_foundation_types = collector_wall_foundation_types.OfClass(typeof(WallFoundationType))
                            .ToElements().Cast<WallFoundationType>();
                        foreach (WallFoundationType wft in wall_foundation_types)
                        {
                            string name = wft.Name;
                            if (name.Contains(prefix_old))
                            {
                                string name_new = name.Replace(prefix_old, prefix_new);
                                wft.Name = name_new;
                                count++;
                            }
                        }
                    }

                    trans.Commit();

                    if (count == 0)
                    {
                        TaskDialog.Show("Names Prefix Change", "No prefixes changed");
                    }
                    else
                    {
                        TaskDialog.Show("Names Prefix Change", string.Format("{0} prefixes changed", count.ToString()));
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
