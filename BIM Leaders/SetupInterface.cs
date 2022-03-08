using Autodesk.Revit.UI;
using BIM_Leaders_UI;
using BIM_Leaders_Core;

namespace BIM_Leaders
{
    public class SetupInterface
    {
        public SetupInterface() { }
        /// <summary>
        /// Creating Ribbon tabs, panels and buttons.
        /// </summary>
        public void Initialize(UIControlledApplication application)
        {
            // Create Ribbon Tab
            string tabName = "BIM Leaders";
            application.CreateRibbonTab(tabName);
            //string path = Assembly.GetExecutingAssembly().Location;

            // Create Ribbon Panel
            string panelName01 = "Standards";
            RibbonPanel panel01 = application.CreateRibbonPanel(tabName, panelName01);

            // Populate button data model
            RevitPushButtonDataModel buttonData0101 = new RevitPushButtonDataModel
            {
                Label = "Checker",
                Panel = panel01,
                ToolTip = "Check file for standards, model and codes.",
                LongDescription = "Check which categories need to be checked for prefix in naming. Also check model issues, and problems with national codes.",
                CommandNamespacePath = Checker.GetPath(),
                IconImageName = "BIM_Leaders_Checker.png",
                //TooltipImageName = "BIM_Leaders_Names_Prefix_Change.png"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData0101);

            // Populate button data model
            RevitPushButtonDataModel button_1_2_data = new RevitPushButtonDataModel
            {
                Label = "Solve\r\nWarnings",
                Panel = panel01,
                ToolTip = "Solve warnings automatically.",
                LongDescription = "Usable for only warnings that don't need user's decision.",
                CommandNamespacePath = WarningsSolve.GetPath(),
                IconImageName = "BIM_Leaders_Warnings.png",
                //TooltipImageName = "BIM_Leaders_Names_Prefix_Change.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_2_data);

            // Populate button data model
            RevitPushButtonDataModel button_1_3_data = new RevitPushButtonDataModel
            {
                Label = "Names Prefix\r\nChange",
                Panel = panel01,
                ToolTip = "Change all names prefixes.",
                LongDescription = "Check which categories need to be renamed. Also can be useful to change part of the names in the middle or at the end.",
                CommandNamespacePath = NamesPrefixChange.GetPath(),
                IconImageName = "BIM_Leaders_Names_Prefix_Change.png",
                //TooltipImageName = "BIM_Leaders_Names_Prefix_Change.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_3_data);

            // Populate button data model
            RevitPushButtonDataModel button_1_4_data = new RevitPushButtonDataModel
            {
                Label = "Delete Unused\r\nLine Styles",
                Panel = panel01,
                ToolTip = "Delete all unused line styles.",
                LongDescription = "Line styles cannot be deleted via Purge function. Use this command to purge all linestyles that are unneeded.",
                CommandNamespacePath = LinestylesUnusedDelete.GetPath(),
                IconImageName = "BIM_Leaders_Linestyles_Unused_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linestyles_Unused_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_4_data);

            // Populate button data model
            RevitPushButtonDataModel button_1_5_data = new RevitPushButtonDataModel
            {
                Label = "Delete \r\nLine Patterns",
                Panel = panel01,
                ToolTip = "Delete all line patterns that contains input string.",
                LongDescription = "Can be useful to delete IMPORT line patterns after importing DWG files.",
                CommandNamespacePath = LinetypesDelete.GetPath(),
                IconImageName = "BIM_Leaders_Linetypes_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linetypes_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_5_data);

            // Create Pull-down button
            RevitPulldownButtonDataModel button_1_6_data = new RevitPulldownButtonDataModel
            {
                Label = "Walls\r\nCheck",
                Panel = panel01,
                ToolTip = "Check walls for arrangement and angles.",
                LongDescription = "Useful for accuracy improvements to see what walls need to be checked.",
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png"
            };
            // Create button from provided data
            PulldownButton button_1_5 = RevitPulldownButton.Create(button_1_6_data);

            RevitPushButtonDataModel button_1_6_1_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nParallel",
                Panel = panel01,
                ToolTip = "Walls parallel check.",
                LongDescription = "Creates wall checking filter. All non-parallel and non-perpendicular walls will be colored.",
                CommandNamespacePath = WallsParallel.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(button_1_6_1_data, button_1_5);

            // Populate button data model
            RevitPushButtonDataModel button_1_6_2_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nArranged",
                Panel = panel01,
                ToolTip = "Walls arrangment check.",
                LongDescription = "Creates wall checking filter. Will be colored: all non-parallel and non-perpendicular walls; all normally angled walls that have distance to intersection with accuracy less than 1 cm.",
                CommandNamespacePath = WallsArranged.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(button_1_6_2_data, button_1_5);

            panel01.AddSeparator();

            // Populate button data model
            RevitPushButtonDataModel button_1_7_data = new RevitPushButtonDataModel
            {
                Label = "Help\r\nStandards",
                Panel = panel01,
                ToolTip = "Standards.",
                LongDescription = "Help!",
                CommandNamespacePath = HelpStandards.GetPath(),
                IconImageName = "BIM_Leaders_Help_Standards.png",
                //TooltipImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_7_data);


            // Create Ribbon Panel
            string panel_2_name = "DWG";
            RibbonPanel panel_2 = application.CreateRibbonPanel(tabName, panel_2_name);

            // Populate button data model
            RevitPushButtonDataModel button_2_1_data = new RevitPushButtonDataModel
            {
                Label = "Find DWG",
                Panel = panel_2,
                ToolTip = "Shows information about all DWG files imports.",
                LongDescription = "Shows name of the import, view on which it lays on (if the import is 2D), and import type (Import or Link).",
                CommandNamespacePath = DwgViewFound.GetPath(),
                IconImageName = "BIM_Leaders_DWG_View_Found.png",
                //TooltipImageName = "BIM_Leaders_DWG_View_Found.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_2_1_data);

            // Populate button data model
            RevitPushButtonDataModel button_2_2_data = new RevitPushButtonDataModel
            {
                Label = "Delete DWG\r\nby Name",
                Panel = panel_2,
                ToolTip = "Delete DWG by selected name on all views.",
                LongDescription = "Usable to delete imported DWG, because it can be on many views, and it takes a time to find all those views and delete the DWG manually.",
                CommandNamespacePath = DwgNameDelete.GetPath(),
                IconImageName = "BIM_Leaders_DWG_Name_Delete.png",
                //TooltipImageName = "BIM_Leaders_DWG_Name_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_2_2_data);




            // Create Ribbon Panel
            string panel_3_name = "Element";
            RibbonPanel panel_3 = application.CreateRibbonPanel(tabName, panel_3_name);

            // Populate button data model
            RevitPushButtonDataModel button_3_1_data = new RevitPushButtonDataModel
            {
                Label = "Remove Paint",
                Panel = panel_3,
                ToolTip = "Remove paint from all faces of element.",
                LongDescription = "Usable if need to clear paint from all faces of some element. Usable only for Paint tool.",
                CommandNamespacePath = ElementPaintRemove.GetPath(),
                IconImageName = "BIM_Leaders_Element_Paint_Remove.png",
                //TooltipImageName = "BIM_Leaders_Element_Paint_Remove.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_3_1_data);

            // Populate button data model
            RevitPushButtonDataModel button_3_2_data = new RevitPushButtonDataModel
            {
                Label = "Join",
                Panel = panel_3,
                ToolTip = "Joins all walls and floors on a section view.",
                LongDescription = "Usable only on a section view. Joins may be reviewed for correct joining order.",
                CommandNamespacePath = ElementsJoin.GetPath(),
                IconImageName = "BIM_Leaders_Elements_Join.png",
                //TooltipImageName = "BIM_Leaders_Element_Paint_Remove.png"
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_3_2_data);




            // Create Ribbon Panel
            string panel_4_name = "Family";
            RibbonPanel panel_4 = application.CreateRibbonPanel(tabName, panel_4_name);

            // Populate button data model
            RevitPushButtonDataModel button_4_1_data = new RevitPushButtonDataModel
            {
                Label = "Select\r\nVoids",
                Panel = panel_4,
                ToolTip = "Select voids in a current family.",
                LongDescription = "Usable if need to select void geometry. This is hard if voids not joined with family geometry.",
                CommandNamespacePath = FamilyVoidsSelect.GetPath(),
                IconImageName = "BIM_Leaders_Familiy_Voids_Select.png",
                //TooltipImageName = "BIM_Leaders_Familiy_Voids_Select.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(button_4_1_data);

            // Populate button data model
            RevitPushButtonDataModel button_4_2_data = new RevitPushButtonDataModel
            {
                Label = "Find\r\nZero",
                Panel = panel_4,
                ToolTip = "Find Femily Zero.",
                LongDescription = "Creates lines around zero coordinates in a current family. Usable for Profile family and other annotation family types.",
                CommandNamespacePath = FamilyZeroCoordinates.GetPath(),
                IconImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                //TooltipImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(button_4_2_data);

            // Populate button data model
            RevitPushButtonDataModel button_4_3_data = new RevitPushButtonDataModel
            {
                Label = "Set\r\nParameter",
                Panel = panel_4,
                ToolTip = "Batch set parameter values to all types.",
                LongDescription = "Usable if family has many types, and the same value need to be set to all of them.",
                CommandNamespacePath = FamilyParameterSet.GetPath(),
                IconImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                //TooltipImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(button_4_3_data);




            // Create Ribbon Panel
            string panel_5_name = "Annotate";
            RibbonPanel panel_5 = application.CreateRibbonPanel(tabName, panel_5_name);

            // Populate button data model
            RevitPushButtonDataModel button_5_1_data = new RevitPushButtonDataModel
            {
                Label = "Dimensions\r\nCheck",
                Panel = panel_5,
                ToolTip = "Check if walls are dimensioned.",
                LongDescription = "Creates a selection filter on a view that shows non-dimensioned walls.",
                CommandNamespacePath = DimensionsPlanCheck.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Plan_Check.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_1_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_2_data = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nSection",
                Panel = panel_5,
                ToolTip = "Dimensions or elevation spots on section.",
                LongDescription = "Automatically puts annotations on a current section. Select a vertical line as a reference for annotations arrangement.",
                CommandNamespacePath = DimensionSectionFloors.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_2_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_3_data = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nLandings",
                Panel = panel_5,
                ToolTip = "Dimensions stairs landings on section.",
                LongDescription = "Automatically puts dimensions on a current section. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = DimensionStairsLandings.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Stairs_Landings.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Stairs_Landings.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_3_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_4_data = new RevitPushButtonDataModel
            {
                Label = "Steps\r\nEnumerate",
                Panel = panel_5,
                ToolTip = "Enumerate Stairs Steps.",
                LongDescription = "Can be useful on section views. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = StairsStepsEnumerate.GetPath(),
                IconImageName = "BIM_Leaders_Stairs_Steps_Enumerate.png",
                //TooltipImageName = "BIM_Leaders_Stairs_Steps_Enumerate.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_4_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_5_data = new RevitPushButtonDataModel
            {
                Label = "Align\r\nGrids",
                Panel = panel_5,
                ToolTip = "Align Grid Ends.",
                LongDescription = "Can be useful on elevation and section views. Bubbles on ends of the grids can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = GridsAlign.GetPath(),
                IconImageName = "BIM_Leaders_Grids_Align.png",
                //TooltipImageName = "BIM_Leaders_Grids_Align.png"
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSectionOrElevation"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_5_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_6_data = new RevitPushButtonDataModel
            {
                Label = "Align\r\nLevels",
                Panel = panel_5,
                ToolTip = "Align Levels Ends.",
                LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = LevelsAlign.GetPath(),
                IconImageName = "BIM_Leaders_Levels_Align.png",
                //TooltipImageName = "BIM_Leaders_Levels_Align.png"
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSectionOrElevation"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_6_data);

            // Populate button data model
            RevitPushButtonDataModel button_5_7_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nCompare",
                Panel = panel_5,
                ToolTip = "Compare Walls for Permit Change Drawing.",
                LongDescription = "Can be useful on plan views. Select a Revit link after settings entering.",
                CommandNamespacePath = WallsCompare.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Compare.png",
                //TooltipImageName = "BIM_Leaders_Walls_Compare.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_7_data);
        }
    }
}
