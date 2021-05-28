using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BIM_Leaders_UI;
using BIM_Leaders_Resources;
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
            string path = Assembly.GetExecutingAssembly().Location;

            // Create Ribbon Panel
            string panel_1_name = "Standards";
            var panel_1 = application.CreateRibbonPanel(tab_name, panel_1_name);

            // Populate button data model
            var button_1_1_data = new RevitPushButtonDataModel
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
            var button_1_1 = RevitPushButton.Create(button_1_1_data);

            // Populate button data model
            var button_1_2_data = new RevitPushButtonDataModel
            {
                Label = "Delete Unused\r\nLinestyles",
                Panel = panel_1,
                ToolTip = "Delete all unused linestyles.",
                LongDescription = "Linestyles cannot be deleted via Purge function. Use this command to purge all linestyles that are unneeded.",
                CommandNamespacePath = Linestyles_Unused_Delete.GetPath(),
                IconImageName = "BIM_Leaders_Linestyles_Unused_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linestyles_Unused_Delete.png"
            };
            // Create button from provided data
            var button_1_2 = RevitPushButton.Create(button_1_2_data);

            // Populate button data model
            var button_1_3_data = new RevitPushButtonDataModel
            {
                Label = "Delete IMPORT\r\nLinetypes",
                Panel = panel_1,
                ToolTip = "Delete all linetypes of the given name.",
                LongDescription = "Can be useful to delete IMPORT linetypes after importing DWG files.",
                CommandNamespacePath = Linetypes_IMPORT_Delete.GetPath(),
                IconImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png",
                //TooltipImageName = "BIM_Leaders_Linetypes_IMPORT_Delete.png"
            };
            // Create button from provided data
            var button_1_3 = RevitPushButton.Create(button_1_3_data);

            // Populate button data model
            var button_1_4_data = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nParralel",
                Panel = panel_1,
                ToolTip = "Walls parallel check.",
                LongDescription = "Creates wall checking filter. All non-parallel and non-perpendicular walls will be colored.",
                CommandNamespacePath = Walls_Parallel.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                //TooltipImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            var button_1_4 = RevitPushButton.Create(button_1_4_data);

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
            var button_2_1 = RevitPushButton.Create(button_2_1_data);

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
            var button_2_2 = RevitPushButton.Create(button_2_2_data);

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
            var button_3_1 = RevitPushButton.Create(button_3_1_data);

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
            var button_4_1 = RevitPushButton.Create(button_4_1_data);

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
            var button_4_2 = RevitPushButton.Create(button_4_2_data);

            // Create Ribbon Panel
            string panel_5_name = "Annotate";
            var panel_5 = application.CreateRibbonPanel(tab_name, panel_5_name);

            // Populate button data model
            var button_5_1_data = new RevitPushButtonDataModel
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
            var button_5_1 = RevitPushButton.Create(button_5_1_data);

            // Populate button data model
            var button_5_2_data = new RevitPushButtonDataModel
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
            var button_5_2 = RevitPushButton.Create(button_5_2_data);

            // Populate button data model
            var button_5_3_data = new RevitPushButtonDataModel
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
            var button_5_3 = RevitPushButton.Create(button_5_3_data);

            // Populate button data model
            var button_5_4_data = new RevitPushButtonDataModel
            {
                Label = "Align\r\nLevels",
                Panel = panel_5,
                ToolTip = "Align Levels Ends.",
                LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = Levels_Align.GetPath(),
                IconImageName = "BIM_Leaders_Levels_Align.png",
                //TooltipImageName = "BIM_Leaders_Levels_Align.png"
            };
            // Create button from provided data
            var button_5_4 = RevitPushButton.Create(button_5_4_data);

            // Populate button data model
            var button_5_5_data = new RevitPushButtonDataModel
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
            var button_5_5 = RevitPushButton.Create(button_5_5_data);

            // Populate button data model
            var button_5_6_data = new RevitPushButtonDataModel
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
            var button_5_6 = RevitPushButton.Create(button_5_6_data);
        }
    }
}
