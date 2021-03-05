using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Check : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Options
            Options opt = new Options();
            opt.ComputeReferences = false;
            opt.View = doc.ActiveView;
            opt.IncludeNonVisibleObjects = true;

            try
            {
                // Getting input data from user
                string prefix = "PRE_";
                List<bool> categories = Enumerable.Repeat(false, 24).ToList();
                List<bool> model = Enumerable.Repeat(false, 5).ToList();
                List<bool> codes = Enumerable.Repeat(false, 2).ToList();

                using (Check_Form form = new Check_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        prefix = form.Result_prefix();
                        categories = form.Result_categories();
                        model = form.Result_model();
                        codes = form.Result_codes();
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }

                int count_prefixes = 0;
                int count_groups = 0;
                int count_groups_unused = 0;
                int count_groups_unpinned = 0;
                int count_groups_excluded = 0;
                int count_linestyles = 0;
                int count_rooms_placement = 0;
                int count_rooms_intersect = 0;
                int count_warnings = 0;
                int count_walls_interior = 0;
                int count_stairs_formula = 0;
                int count_height_landings = 0;
                int count_height_runs = 0;

                using (Transaction trans = new Transaction(doc, "Check"))
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (FamilySymbol fs in family_symbols)
                        {
                            string name = fs.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (RailingType rt in railing_types)
                        {
                            string name = rt.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (GutterType gt in gutter_types)
                        {
                            string name = gt.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (RoofType rft in roof_types)
                        {
                            string name = rft.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (ViewPlan vp in views_plan)
                        {
                            string name = vp.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (ViewSchedule vs in views_schedule)
                        {
                            string name = vs.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                        foreach (ViewSection vsn in views_section)
                        {
                            string name = vsn.Name;
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
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
                            if (!name.StartsWith(prefix))
                            {
                                count_prefixes++;
                            }
                        }
                    }

                    // Groups check
                    if (model[0])
                    {
                        FilteredElementCollector collector_group_types_0 = new FilteredElementCollector(doc);
                        IEnumerable<GroupType> group_types_0 = collector_group_types_0.OfClass(typeof(GroupType))
                            .ToElements().Cast<GroupType>();
                        FilteredElementCollector collector_groups = new FilteredElementCollector(doc);
                        IEnumerable<Group> groups = collector_groups.OfClass(typeof(Group))
                            .ToElements().Cast<Group>();
                        foreach (GroupType gt in group_types_0)
                        {
                            // Check size of GroupSet which given from GroupType
                            if (gt.Groups.Size == 0)
                            {
                                count_groups_unused++;
                            }
                        }
                        foreach (Group g in groups)
                        {
                            // Check for unpinned groups
                            if (!g.Pinned)
                            {
                                count_groups_unpinned++;
                            }
                            // Check if group has excluded elements
                            if (g.Name.EndsWith("(members excluded)"))
                            {
                                count_groups_excluded++;
                            }
                            count_groups++;
                        }
                    }

                    // Line Styles Unused check
                    if (model[1])
                    {
                        FilteredElementCollector collector_lines = new FilteredElementCollector(doc);
                        IEnumerable<Line> lines = collector_lines.OfClass(typeof(Line)).WhereElementIsNotElementType().ToElements().Cast<Line>();
                        List<ElementId> line_styles_used = new List<ElementId>();
                        foreach (Line l in lines)
                        {
                            ElementId line_style = l.GraphicsStyleId;
                            if (!line_styles_used.Contains(line_style))
                            {
                                line_styles_used.Add(line_style);
                            }
                        }
                        // Selecting all line styles in the project
                        CategoryNameMap line_styles_all = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).SubCategories;
                        List<ElementId> line_styles = new List<ElementId>();
                        foreach (Category c in line_styles_all)
                        {
                            line_styles.Add(c.Id);
                        }
                        foreach (ElementId i in line_styles)
                        {
                            if (!line_styles_used.Contains(i))
                            {
                                count_linestyles++;
                            }
                        }
                    }

                    // Rooms check
                    if (model[2])
                    {
                        RoomFilter filter = new RoomFilter();
                        FilteredElementCollector collector_rooms = new FilteredElementCollector(doc);
                        IEnumerable<Room> rooms = collector_rooms.OfClass(typeof(Room)).WherePasses(filter).ToElements().Cast<Room>();

                        Options opt = new Options();
                        List<Solid> solids = new List<Solid>();
                        foreach (Room r in rooms)
                        {
                            if (r.Location is null)
                            {
                                count_rooms_placement++;
                            }
                            // Checking for volumes overlap
                            else
                            {
                                foreach (Solid s1 in r.get_Geometry(opt))
                                {
                                    foreach (Solid s2 in solids)
                                    {
                                        Solid solid = BooleanOperationsUtils.ExecuteBooleanOperation(s1, s2, BooleanOperationsType.Intersect);
                                        if (solid.Volume > 0)
                                        {
                                            count_rooms_intersect++;
                                        }
                                    }
                                    solids.Add(s1);
                                }
                            }
                        }
                    }

                    // Warnings check
                    if (model[3])
                    {
                        IList<FailureMessage> warnings = doc.GetWarnings();
                        count_warnings = warnings.Count();
                    }

                    // Exterior walls check
                    if (model[4])
                    {
                        FilteredElementCollector collector_views = new FilteredElementCollector(doc);
                        View3D v = collector_views.OfClass(typeof(View3D)).ToElements().Cast<View3D>().First();
                        ElementId v_id = v.GetTypeId();
                        View3D view_new = View3D.CreateIsometric(doc, v_id);

                        BoundingBoxXYZ box = view_new.GetSectionBox(); // CHECK !!!

                        // Get perimeter points and lines
                        XYZ p1 = box.Min;
                        XYZ p2 = new XYZ(box.Min.X, box.Max.Y, box.Min.Z);
                        XYZ p3 = new XYZ(box.Max.X, box.Max.Y, box.Min.Z);
                        XYZ p4 = new XYZ(box.Max.X, box.Min.Y, box.Min.Z);

                        CurveLoop curve = new CurveLoop();
                        curve.Append(Line.CreateBound(p1, p2));
                        curve.Append(Line.CreateBound(p2, p3));
                        curve.Append(Line.CreateBound(p3, p4));
                        curve.Append(Line.CreateBound(p4, p1));

                        // Get the lowest level
                        FilteredElementCollector collector_levels = new FilteredElementCollector(doc);
                        IEnumerable<Level> levels = collector_levels.OfClass(typeof(Level)).ToElements().Cast<Level>();
                        Level level = levels.First();
                        double elev = levels.First().Elevation;
                        foreach (Level l in levels)
                        {
                            if (l.Elevation < elev)
                            {
                                level = l;
                                elev = l.Elevation;
                            }
                        }
                        ElementId level_id = level.Id;

                        // Create perimeter walls
                        List<Wall> walls_to_delete = new List<Wall>();
                        foreach (Line l in curve)
                        {
                            Wall w = Wall.Create(doc, l, level_id, false);
                            Parameter p = w.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET);
                            p.Set(1000);
                            walls_to_delete.Add(w); // For deleting
                        }

                        // Create room
                        UV point = new UV(box.Min.X + 10, box.Min.Y + 10);
                        Room room = doc.Create.NewRoom(level, point);

                        // Get outer walls
                        List<Element> walls = new List<Element>();
                        SpatialElementBoundaryOptions seb_options = new SpatialElementBoundaryOptions();
                        IList<IList<BoundarySegment>> segments = room.GetBoundarySegments(seb_options);
                        foreach (IList<BoundarySegment> i in segments)
                        {
                            foreach (BoundarySegment j in i)
                            {
                                Element element = doc.GetElement(j.ElementId);
                                try
                                {
                                    if (element.Category.Name == "Walls")
                                    {
                                        walls.Add(element);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }

                        // Check if walls are Exterior
                        foreach (Wall w in walls)
                        {
                            if (w.WallType.Function == WallFunction.Interior)
                            {
                                count_walls_interior++;
                            }
                        }

                        // Remove created 4 walls if needed
                        if (walls_to_delete[0].WallType.Function == WallFunction.Interior)
                        {
                            count_walls_interior = count_walls_interior - 4;
                        }

                        // Deleting
                        doc.Delete(room.Id);
                        foreach (Wall i in walls_to_delete)
                        {
                            doc.Delete(i.Id);
                        }
                    }

                    // Checking stairs formula
                    if (codes[0])
                    {
                        FilteredElementCollector collector_stairs = new FilteredElementCollector(doc);
                        IEnumerable<Stairs> stairs = collector_stairs.OfClass(typeof(Stairs)).ToElements().Cast<Stairs>();

                        // Check if stairs steps are right height and depth
                        foreach (Stairs s in stairs)
                        {
                            double step_height = UnitUtils.ConvertToInternalUnits(s.ActualRiserHeight, DisplayUnitType.DUT_CENTIMETERS);
                            double step_depth = UnitUtils.ConvertToInternalUnits(s.ActualTreadDepth, DisplayUnitType.DUT_CENTIMETERS);

                            double r = 2 * step_height + step_depth;

                            if (r < 61 | r > 63 | step_height < 10 | step_height > 17.5 | step_depth < 26)
                            {
                                count_stairs_formula++;
                            }
                        }
                    }

                    // Checking stairs head height
                    if (codes[1])
                    {
                        double height_offset = UnitUtils.ConvertToInternalUnits(10, DisplayUnitType.DUT_CENTIMETERS);
                        double height = UnitUtils.ConvertToInternalUnits(head_height, DisplayUnitType.DUT_CENTIMETERS) - height_offset;

                        FilteredElementCollector collector_stairs = new FilteredElementCollector(doc);
                        IEnumerable<Stairs> stairs = collector_stairs.OfClass(typeof(Stairs)).ToElements().Cast<Stairs>();

                        // Check
                        foreach (Stairs s in stairs)
                        {
                            // Get landings
                            ICollection<ElementId> landings_ids = s.GetStairsLandings();
                            List<StairsLanding> landings = new List<StairsLanding>();
                            foreach (ElementId i in landings_ids)
                            {
                                landings.Add(doc.GetElement(i) as StairsLanding);
                            }

                            // Check landings geometry
                            foreach (StairsLanding l in landings)
                            {
                                double landing_z = l.get_BoundingBox(doc.ActiveView).Max.Z;
                                double landing_elev = l.BaseElevation;
                                CurveLoop landing_p = l.GetFootprintBoundary();
                                List<CurveLoop> looplist = new List<CurveLoop>();
                                looplist.Add(landing_p);

                                // Get extrusion vector, only direction is needed but for sure the point is on 0 point of solid bottom but Z is on solid top
                                XYZ extrusion_dir = new XYZ(0, 0, 1);

                                // Create solid, with solid height of "height_offset"
                                Solid solid_landing = GeometryCreationUtilities.CreateExtrusionGeometry(looplist, extrusion_dir, height_offset);
                                // Transform solid to the needed height
                                Transform t = Transform.CreateTranslation(new XYZ(0, 0, height + landing_elev));
                                Solid transformed = SolidUtils.CreateTransformed(solid_landing, t);

                                // Create filter to get intersecting elements
                                ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(transformed);
                                FilteredElementCollector collector_intersect = new FilteredElementCollector(doc);
                                IList<Element> el = collector_intersect.WherePasses(filter).ToElements();

                                count_height_landings += el.Count;
                            }

                            // Get runs
                            ICollection<ElementId> runs_ids = s.GetStairsRuns();
                            List<StairsRun> runs = new List<StairsRun>();
                            foreach (ElementId i in runs_ids)
                            {
                                runs.Add(doc.GetElement(i) as StairsRun);
                            }

                            // Check runs geometry
                            foreach (StairsRun r in runs)
                            {
                                GeometryElement run_geom = r.get_Geometry(opt);

                                // Get run solid that on top (in run might be second solid for topping)
                                double g_bb_prev = -10000;
                                List<FaceArray> face_arrays = new List<FaceArray>();
                                foreach (Solid g in run_geom)
                                {
                                    if (g.Volume > 0)
                                    {
                                        double g_bb = g.GetBoundingBox().Max.Z;
                                        if (g_bb > g_bb_prev)
                                        {
                                            face_arrays.Add(g.Faces);
                                        }
                                    }
                                }

                                // Filter faces by normal going up
                                List<Face> run_faces = new List<Face>();
                                foreach (FaceArray f_a in face_arrays)
                                {
                                    foreach (PlanarFace f in f_a)
                                    {
                                        double f_n_x = f.FaceNormal.X;
                                        double f_n_y = f.FaceNormal.Y;
                                        double f_n_z = f.FaceNormal.Z;
                                        if (f_n_x == 0 && f_n_y == 0 && f_n_z == 1)
                                        {
                                            run_faces.Add(f);
                                        }
                                    }
                                }

                                // Making solid and check
                                foreach (PlanarFace r_f in run_faces)
                                {
                                    IList<CurveLoop> run_p = r_f.GetEdgesAsCurveLoops();
                                    XYZ extrusion_dir = new XYZ(0, 0, 1);

                                    // Create solid on "height" height from landing, with solid height of "height_offset"
                                    Solid solid_run = GeometryCreationUtilities.CreateExtrusionGeometry(run_p, extrusion_dir, height_offset);
                                    // Transform solid to the needed height
                                    Transform t_f = Transform.CreateTranslation(new XYZ(0, 0, height));
                                    Solid transformed = SolidUtils.CreateTransformed(solid_run, t_f);

                                    // Create filter to get intersecting elements
                                    ElementIntersectsSolidFilter face_filter = new ElementIntersectsSolidFilter(transformed);
                                    FilteredElementCollector collector_intersect = new FilteredElementCollector(doc);
                                    IList<Element> er = collector_intersect.WherePasses(face_filter).ToElements();

                                    count_height_runs += er.Count;
                                }
                            }
                        }

                        trans.Commit();

                        if (count_prefixes == 0 && count_groups_unused == 0 && count_groups_unpinned == 0 && count_groups_excluded == 0)
                        {
                            TaskDialog.Show("Check", "No issues found");
                        }
                        else
                        {
                            string mes = "";
                            if (!(count_prefixes == 0))
                            {
                                mes += string.Format("{0} prefixes wrong.", count_prefixes.ToString());
                            }
                            if (!(count_groups_unused == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} of {1} groups are not pinned.", count_groups_unused.ToString(), count_groups.ToString());
                            }
                            if (!(count_groups_unpinned == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} of {1} groups are not used.", count_groups_unpinned.ToString(), count_groups.ToString());
                            }
                            if (!(count_groups_excluded == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} of {1} group instances are with excluded elements.", count_groups_excluded.ToString(), count_groups.ToString());
                            }
                            if (!(count_linestyles == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} line styles are unused.", count_linestyles.ToString());
                            }
                            if (!(count_rooms_placement == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} rooms are not placed.", count_rooms_placement.ToString());
                            }
                            if (!(count_rooms_intersect == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} rooms overlap.", count_rooms_intersect.ToString());
                            }
                            if (!(count_warnings == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} warnings in the project.", count_warnings.ToString());
                            }
                            if (!(count_walls_interior == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} exterior walls have interior type.", count_walls_interior.ToString());
                            }
                            if (!(count_stairs_formula == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} stairs have bad formula.", count_stairs_formula.ToString());
                            }
                            if (!(count_height_landings == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} stairs landings have too low head height.", count_height_landings.ToString());
                            }
                            if (!(count_height_runs == 0))
                            {
                                if (!(mes.Length == 0))
                                {
                                    mes += " ";
                                }
                                mes += string.Format("{0} stairs runs have too low head height.", count_height_runs.ToString());
                            }
                        }
                    }
                    return Result.Succeeded;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
