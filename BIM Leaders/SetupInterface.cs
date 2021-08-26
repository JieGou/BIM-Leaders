using System.Reflection;
using Autodesk.Revit.UI;
using BIM_Leaders_UI;
using BIM_Leaders_Core;

namespace BIM_Leaders
{
    public class SetupInterface
    {
        public SetupInterface()
        {
            
        }
        public void Initialize(UIControlledApplication application)
        {
            // Create Ribbon Tab
            string tab_name = "BIM Leaders";
            application.CreateRibbonTab(tab_name);
            //string path = Assembly.GetExecutingAssembly().Location;

            // Create Ribbon Panel
            string panel_1_name = "Standards";
            RibbonPanel panel_1 = application.CreateRibbonPanel(tab_name, panel_1_name);

            // Populate button data model
            var button_1_1_data = new RevitPushButtonDataModel
            {
                Label = "Checker",
                Panel = panel_1,
                ToolTip = "Check file for standards, model and codes.",
                LongDescription = "Check which categories need to be checked for prefix in naming. Also check model issues, and problems with national codes.",
                CommandNamespacePath = Checker.GetPath(),
                IconImageName = "BIM_Leaders_Checker.png",
                //TooltipImageName = "BIM_Leaders_Names_Prefix_Change.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_1_data);

            // Populate button data model
            var button_1_2_data = new RevitPushButtonDataModel
            {
                Label = "Names Prefix\r\nChange",
                Panel = panel_1,
                ToolTip = "Change all names prefixes.",
                LongDescription = "Check which categories need to be renamed. Also can be useful to change part of the names in the middle or at the end.",
                CommandNamespacePath = Names_Prefix_Change.GetPath(),
                IconImageName = "BIM_Leaders_Names_Prefix_Change.png",
                //TooltipImageName = "BIM_Leaders_Names_Prefix_Change.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_2_data);

            // Populate button data model
            var button_1_3_data = new RevitPushButtonDataModel
            {
                Label = "Delete Unused\r\nLine Styles",
                Panel = panel_1,
                ToolTip = "Delete all unused line styles.",
                LongDescription = "Line styles cannot be deleted via Purge function. Use this command to purge all linestyles that are unneeded.",
                CommandNamespacePath = Linestyles_Unused_Delete.GetPath(),
                IconImageName = "BIM_Leaders_Linestyles_Unused_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linestyles_Unused_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_3_data);

            // Populate button data model
            var button_1_4_data = new RevitPushButtonDataModel
            {
                Label = "Delete IMPORT\r\nLine Patterns",
                Panel = panel_1,
                ToolTip = "Delete all line patterns that contains input string.",
                LongDescription = "Can be useful to delete IMPORT line patterns after importing DWG files.",
                CommandNamespacePath = Linetypes_IMPORT_Delete.GetPath(),
                IconImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_4_data);

            // Create Pull-down button
            var button_1_5_data = new RevitPulldownButtonDataModel
            {
                Label = "Walls\r\nCheck",
                Panel = panel_1,
                ToolTip = "Check walls for arrangement and angles.",
                LongDescription = "Useful for accuracy improvements to see what walls need to be checked.",
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png"
            };
            // Create button from provided data
            var button_1_5 = RevitPulldownButton.Create(button_1_5_data);

            var button_1_5_1_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nParallel",
                Panel = panel_1,
                ToolTip = "Walls parallel check.",
                LongDescription = "Creates wall checking filter. All non-parallel and non-perpendicular walls will be colored.",
                CommandNamespacePath = Walls_Parallel.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(button_1_5_1_data, button_1_5);

            // Populate button data model
            var button_1_5_2_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nArranged",
                Panel = panel_1,
                ToolTip = "Walls arrangment check.",
                LongDescription = "Creates wall checking filter. Will be colored: all non-parallel and non-perpendicular walls; all normally angled walls that have distance to intersection with accuracy less than 1 cm.",
                CommandNamespacePath = Walls_Arranged.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(button_1_5_2_data, button_1_5);

            panel_1.AddSeparator();

            // Populate button data model
            var button_1_6_data = new RevitPushButtonDataModel
            {
                Label = "Help\r\nStandards",
                Panel = panel_1,
                ToolTip = "Standards.",
                LongDescription = "Help!",
                CommandNamespacePath = Help_Standards.GetPath(),
                IconImageName = "BIM_Leaders_Help_Standards.png",
                //TooltipImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_1_6_data);


            // Create Ribbon Panel
            string panel_2_name = "DWG";
            var panel_2 = application.CreateRibbonPanel(tab_name, panel_2_name);

            // Populate button data model
            var button_2_1_data = new RevitPushButtonDataModel
            {
                Label = "Find DWG",
                Panel = panel_2,
                ToolTip = "Shows information about all DWG files imports.",
                LongDescription = "Shows name of the import, view on which it lays on (if the import is 2D), and import type (Import or Link).",
                CommandNamespacePath = DWG_View_Found.GetPath(),
                IconImageName = "BIM_Leaders_DWG_View_Found.png",
                //TooltipImageName = "BIM_Leaders_DWG_View_Found.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_2_1_data);

            // Populate button data model
            var button_2_2_data = new RevitPushButtonDataModel
            {
                Label = "Delete DWG\r\nby Name",
                Panel = panel_2,
                ToolTip = "Delete DWG by selected name on all views.",
                LongDescription = "Usable to delete imported DWG, because it can be on many views, and it takes a time to find all those views and delete the DWG manually.",
                CommandNamespacePath = DWG_Name_Delete.GetPath(),
                IconImageName = "BIM_Leaders_DWG_Name_Delete.png",
                //TooltipImageName = "BIM_Leaders_DWG_Name_Delete.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_2_2_data);




            // Create Ribbon Panel
            string panel_3_name = "Element";
            var panel_3 = application.CreateRibbonPanel(tab_name, panel_3_name);

            // Populate button data model
            var button_3_1_data = new RevitPushButtonDataModel
            {
                Label = "Remove Paint",
                Panel = panel_3,
                ToolTip = "Remove paint from all faces of element.",
                LongDescription = "Usable if need to clear paint from all faces of some element. Usable only for Paint tool.",
                CommandNamespacePath = Element_Paint_Remove.GetPath(),
                IconImageName = "BIM_Leaders_Element_Paint_Remove.png",
                //TooltipImageName = "BIM_Leaders_Element_Paint_Remove.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_3_1_data);

            // Populate button data model
            var button_3_2_data = new RevitPushButtonDataModel
            {
                Label = "Join",
                Panel = panel_3,
                ToolTip = "Joins all walls and floors on a section view.",
                LongDescription = "Usable only on a section view. Joins may be reviewed for correct joining order.",
                CommandNamespacePath = Elements_Join.GetPath(),
                IconImageName = "BIM_Leaders_Elements_Join.png",
                //TooltipImageName = "BIM_Leaders_Element_Paint_Remove.png"
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_3_2_data);




            // Create Ribbon Panel
            string panel_4_name = "Family";
            var panel_4 = application.CreateRibbonPanel(tab_name, panel_4_name);

            // Populate button data model
            var button_4_1_data = new RevitPushButtonDataModel
            {
                Label = "Select\r\nVoids",
                Panel = panel_4,
                ToolTip = "Select voids in a current family.",
                LongDescription = "Usable if need to select void geometry. This is hard if voids not joined with family geometry.",
                CommandNamespacePath = Family_Voids_Select.GetPath(),
                IconImageName = "BIM_Leaders_Familiy_Voids_Select.png",
                //TooltipImageName = "BIM_Leaders_Familiy_Voids_Select.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(button_4_1_data);

            // Populate button data model
            var button_4_2_data = new RevitPushButtonDataModel
            {
                Label = "Find\r\nZero",
                Panel = panel_4,
                ToolTip = "Find Femily Zero.",
                LongDescription = "Creates lines around zero coordinates in a current family. Usable for Profile family and other annotation family types.",
                CommandNamespacePath = Family_Zero_Coordinates.GetPath(),
                IconImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                //TooltipImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(button_4_2_data);




            // Create Ribbon Panel
            string panel_5_name = "Annotate";
            var panel_5 = application.CreateRibbonPanel(tab_name, panel_5_name);

            // Populate button data model
            var button_5_1_data = new RevitPushButtonDataModel
            {
                Label = "Dimensions\r\nCheck",
                Panel = panel_5,
                ToolTip = "Check if walls are dimensioned.",
                LongDescription = "Creates a selection filter on a view that shows non-dimensioned walls.",
                CommandNamespacePath = Dimensions_Plan_Check.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Plan_Check.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_1_data);

            // Populate button data model
            var button_5_2_data = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nSection",
                Panel = panel_5,
                ToolTip = "Dimensions or elevation spots on section.",
                LongDescription = "Automatically puts annotations on a current section. Select a vertical line as a reference for annotations arrangement.",
                CommandNamespacePath = Dimension_Section_Floors.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_2_data);

            // Populate button data model
            var button_5_3_data = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nLandings",
                Panel = panel_5,
                ToolTip = "Dimensions stairs landings on section.",
                LongDescription = "Automatically puts dimensions on a current section. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = Dimension_Stairs_Landings.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Stairs_Landings.png",
                //TooltipImageName = "BIM_Leaders_Dimensions_Stairs_Landings.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_3_data);

            // Populate button data model
            var button_5_4_data = new RevitPushButtonDataModel
            {
                Label = "Align\r\nGrids",
                Panel = panel_5,
                ToolTip = "Align Grid Ends.",
                LongDescription = "Can be useful on elevation and section views. Bubbles on ends of the grids can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = Grids_Align.GetPath(),
                IconImageName = "BIM_Leaders_Grids_Align.png",
                //TooltipImageName = "BIM_Leaders_Grids_Align.png"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_4_data);

            // Populate button data model
            var button_5_5_data = new RevitPushButtonDataModel
            {
                Label = "Align\r\nLevels",
                Panel = panel_5,
                ToolTip = "Align Levels Ends.",
                LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = Levels_Align.GetPath(),
                IconImageName = "BIM_Leaders_Levels_Align.png",
                //TooltipImageName = "BIM_Leaders_Levels_Align.png"
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSectionOrElevation"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_5_data);

            // Populate button data model
            var button_5_6_data = new RevitPushButtonDataModel
            {
                Label = "Steps\r\nEnumerate",
                Panel = panel_5,
                ToolTip = "Enumerate Stairs Steps.",
                LongDescription = "Can be useful on section views. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = Stairs_Steps_Enumerate.GetPath(),
                IconImageName = "BIM_Leaders_Stairs_Steps_Enumerate.png",
                //TooltipImageName = "BIM_Leaders_Stairs_Steps_Enumerate.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_6_data);

            // Populate button data model
            var button_5_7_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nCompare",
                Panel = panel_5,
                ToolTip = "Compare Walls for Permit Change Drawing.",
                LongDescription = "Can be useful on plan views. Select a Revit link after settings entering.",
                CommandNamespacePath = Walls_Compare.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Compare.png",
                //TooltipImageName = "BIM_Leaders_Walls_Compare.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(button_5_7_data);
        }
    }
}
