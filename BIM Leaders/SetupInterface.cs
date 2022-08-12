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

            // Create Ribbon Panels
            CreatePanel01(application, tabName, "Standards");
            CreatePanel02(application, tabName, "DWG");
            CreatePanel03(application, tabName, "Element");
            CreatePanel04(application, tabName, "Family");
            CreatePanel05(application, tabName, "Annotate");
        }

        private void CreatePanel01(UIControlledApplication application, string tabName, string panelName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Populate button data model
            RevitPushButtonDataModel buttonData01 = new RevitPushButtonDataModel
            {
                Label = "Checker",
                Panel = panel,
                ToolTip = "Check file for standards, model and codes.",
                LongDescription = "Check which categories need to be checked for prefix in naming. Also check model issues, and problems with national codes.",
                CommandNamespacePath = Checker.GetPath(),
                IconImageName = "BIM_Leaders_Checker.png",
                //TooltipImageName = "BIM_Leaders_Image.png"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData01);

            // Create Pull-down button
            RevitPulldownButtonDataModel buttonData02 = new RevitPulldownButtonDataModel
            {
                Label = "Walls\r\nCheck",
                Panel = panel,
                ToolTip = "Check walls for arrangement and angles.",
                LongDescription = "Useful for accuracy improvements to see what walls need to be checked.",
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
            };
            // Create button from provided data
            PulldownButton button02 = RevitPulldownButton.Create(buttonData02);

            RevitPushButtonDataModel buttonData0201 = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nParallel",
                Panel = panel,
                ToolTip = "Walls parallel check.",
                LongDescription = "Creates wall checking filter. All non-parallel and non-perpendicular walls will be colored.",
                CommandNamespacePath = WallsParallel.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(buttonData0201, button02);

            // Populate button data model
            RevitPushButtonDataModel buttonData0202 = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nArranged",
                Panel = panel,
                ToolTip = "Walls arrangment check.",
                LongDescription = "Creates wall checking filter. Will be colored: all non-parallel and non-perpendicular walls; all normally angled walls that have distance to intersection with accuracy less than 1 cm.",
                CommandNamespacePath = WallsArranged.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Parallel.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.CreateInPulldown(buttonData0202, button02);

            panel.AddSeparator();

            // Populate button data model
            RevitPushButtonDataModel buttonData03 = new RevitPushButtonDataModel
            {
                Label = "Names\r\nChange",
                Panel = panel,
                ToolTip = "Change all names.",
                LongDescription = "Check which categories need to be renamed. Also can be useful to change part of the names in the middle or at the end.",
                CommandNamespacePath = NamesChange.GetPath(),
                IconImageName = "BIM_Leaders_Names_Change.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData03);

            // Populate button data model
            RevitPushButtonDataModel buttonData04 = new RevitPushButtonDataModel
            {
                Label = "Purge",
                Panel = panel,
                ToolTip = "Purge the model.",
                LongDescription = "Not all categories and elements can be purged with standard purge tool. Use this command for advanced purging.",
                CommandNamespacePath = Purge.GetPath(),
                IconImageName = "BIM_Leaders_Purge.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData04);

            // Populate button data model
            RevitPushButtonDataModel buttonData05 = new RevitPushButtonDataModel
            {
                Label = "Solve\r\nWarnings",
                Panel = panel,
                ToolTip = "Solve warnings automatically.",
                LongDescription = "Usable for only warnings that don't need user's decision.",
                CommandNamespacePath = WarningsSolve.GetPath(),
                IconImageName = "BIM_Leaders_Warnings.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData05);

            panel.AddSeparator();

            // Populate button data model
            RevitPushButtonDataModel buttonData06 = new RevitPushButtonDataModel
            {
                Label = "Journals\r\nAnalyze",
                Panel = panel,
                ToolTip = "Standards.",
                LongDescription = "Analyze the current Revit journal file.",
                CommandNamespacePath = JournalAnalyze.GetPath(),
                IconImageName = "BIM_Leaders_Journal_Analyze.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData06);

            panel.AddSeparator();

            // Populate button data model
            RevitPushButtonDataModel buttonData07 = new RevitPushButtonDataModel
            {
                Label = "Help\r\nStandards",
                Panel = panel,
                ToolTip = "Standards.",
                LongDescription = "Help!",
                CommandNamespacePath = HelpStandards.GetPath(),
                IconImageName = "BIM_Leaders_Help_Standards.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData07);
        }
        private void CreatePanel02(UIControlledApplication application, string tabName, string panelName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Populate button data model
            RevitPushButtonDataModel buttonData01 = new RevitPushButtonDataModel
            {
                Label = "Find DWG",
                Panel = panel,
                ToolTip = "Shows information about all DWG files imports.",
                LongDescription = "Shows name of the import, view on which it lays on (if the import is 2D), and import type (Import or Link).",
                CommandNamespacePath = DwgViewFound.GetPath(),
                IconImageName = "BIM_Leaders_DWG_View_Found.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData01);

            // Populate button data model
            RevitPushButtonDataModel buttonData02 = new RevitPushButtonDataModel
            {
                Label = "Delete DWG\r\nby Name",
                Panel = panel,
                ToolTip = "Delete DWG by selected name on all views.",
                LongDescription = "Usable to delete imported DWG, because it can be on many views, and it takes a time to find all those views and delete the DWG manually.",
                CommandNamespacePath = DwgNameDelete.GetPath(),
                IconImageName = "BIM_Leaders_DWG_Name_Delete.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData02);
        }
        private void CreatePanel03(UIControlledApplication application, string tabName, string panelName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Populate button data model
            RevitPushButtonDataModel buttonData01 = new RevitPushButtonDataModel
            {
                Label = "Remove Paint",
                Panel = panel,
                ToolTip = "Remove paint from all faces of element.",
                LongDescription = "Usable if need to clear paint from all faces of some element. Usable only for Paint tool.",
                CommandNamespacePath = ElementPaintRemove.GetPath(),
                IconImageName = "BIM_Leaders_Element_Paint_Remove.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData01);

            // Populate button data model
            RevitPushButtonDataModel buttonData02 = new RevitPushButtonDataModel
            {
                Label = "Join",
                Panel = panel,
                ToolTip = "Joins all walls and floors on a section view.",
                LongDescription = "Usable only on a section view. Joins may be reviewed for correct joining order.",
                CommandNamespacePath = ElementsJoin.GetPath(),
                IconImageName = "BIM_Leaders_Elements_Join.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData02);

            // Populate button data model
            RevitPushButtonDataModel buttonData03 = new RevitPushButtonDataModel
            {
                Label = "Match\r\nParameters",
                Panel = panel,
                ToolTip = "Transfer instance parameters values from one element to other.",
                LongDescription = "Does not match those parameters: Type, Family and Type, Image.",
                CommandNamespacePath = ElementPropertiesMatch.GetPath(),
                IconImageName = "BIM_Leaders_Element_Properties_Match.png",
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData03);
        }
        private void CreatePanel04(UIControlledApplication application, string tabName, string panelName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Populate button data model
            RevitPushButtonDataModel buttonData01 = new RevitPushButtonDataModel
            {
                Label = "Select\r\nVoids",
                Panel = panel,
                ToolTip = "Select voids in a current family.",
                LongDescription = "Usable if need to select void geometry. This is hard if voids not joined with family geometry.",
                CommandNamespacePath = FamilyVoidsSelect.GetPath(),
                IconImageName = "BIM_Leaders_Family_Voids_Select.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData01);

            // Populate button data model
            RevitPushButtonDataModel buttonData02 = new RevitPushButtonDataModel
            {
                Label = "Find\r\nZero",
                Panel = panel,
                ToolTip = "Find Femily Zero.",
                LongDescription = "Creates lines around zero coordinates in a current family. Usable for Profile family and other annotation family types.",
                CommandNamespacePath = FamilyZeroCoordinates.GetPath(),
                IconImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData02);

            // Populate button data model
            RevitPushButtonDataModel buttonData03 = new RevitPushButtonDataModel
            {
                Label = "Set\r\nParameter",
                Panel = panel,
                ToolTip = "Batch set parameter values to all types.",
                LongDescription = "Usable if family has many types, and the same value need to be set to all of them.",
                CommandNamespacePath = FamilyParameterSet.GetPath(),
                IconImageName = "BIM_Leaders_Family_Zero_Coordinates.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData03);

            // Populate button data model
            RevitPushButtonDataModel buttonData04 = new RevitPushButtonDataModel
            {
                Label = "Change\r\nParameters",
                Panel = panel,
                ToolTip = "Batch change shared parameters to family parameters.",
                LongDescription = "Usable if family has many types, and the same value need to be set to all of them.",
                CommandNamespacePath = FamilyParameterChange.GetPath(),
                IconImageName = "BIM_Leaders_Family_Parameter_Change.png",
                AvailabilityClassName = "BIM_Leaders_Core.DocumentIsFamily"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData04);
        }
        private void CreatePanel05(UIControlledApplication application, string tabName, string panelName)
        {
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Populate button data model
            RevitPushButtonDataModel buttonData01 = new RevitPushButtonDataModel
            {
                Label = "Walls\r\nCompare",
                Panel = panel,
                ToolTip = "Compare Walls for Permit Change Drawing.",
                LongDescription = "Can be useful on plan views. Select a Revit link after settings entering.",
                CommandNamespacePath = WallsCompare.GetPath(),
                IconImageName = "BIM_Leaders_Walls_Compare.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData01);

            // Populate button data model
            RevitPushButtonDataModel buttonData02 = new RevitPushButtonDataModel
            {
                Label = "Dimensions\r\nCheck",
                Panel = panel,
                ToolTip = "Check if walls are dimensioned.",
                LongDescription = "Creates a selection filter on a view that shows non-dimensioned walls.",
                CommandNamespacePath = DimensionsPlanCheck.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Plan_Check.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData02);

            // Populate button data model
            RevitPushButtonDataModel buttonData03 = new RevitPushButtonDataModel
            {
                Label = "Tags\r\nCheck",
                Panel = panel,
                ToolTip = "Check if elements are tagged.",
                LongDescription = "Creates a selection filter on a view that shows non-tagged elements.",
                CommandNamespacePath = TagsPlanCheck.GetPath(),
                IconImageName = "BIM_Leaders_Tags_Plan_Check.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData03);

            // Populate button data model
            RevitPushButtonDataModel buttonData04 = new RevitPushButtonDataModel
            {
                Label = "Dimension\r\nLine",
                Panel = panel,
                ToolTip = "Dimension all walls and columns with given line as a reference.",
                LongDescription = "Creates a dimension line on a current view.",
                CommandNamespacePath = DimensionsPlanLine.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Plan_Line.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData04);

            // Populate button data model
            RevitPushButtonDataModel buttonData05 = new RevitPushButtonDataModel
            {
                Label = "Dimension\r\nPlan",
                Panel = panel,
                ToolTip = "Dimension all walls and columns.",
                LongDescription = "Creates a dimensions on a current view.",
                CommandNamespacePath = DimensionsPlan.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Plan.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsPlan"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData05);

            panel.AddSeparator();

            // Populate button data model
            RevitPushButtonDataModel buttonData06 = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nSection",
                Panel = panel,
                ToolTip = "Dimensions or elevation spots on section.",
                LongDescription = "Automatically puts annotations on a current section. Select a vertical line as a reference for annotations arrangement.",
                CommandNamespacePath = DimensionSectionFloors.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Section_Floors.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData06);

            // Populate button data model
            RevitPushButtonDataModel buttonData07 = new RevitPushButtonDataModel
            {
                Label = "Annotate\r\nLandings",
                Panel = panel,
                ToolTip = "Dimensions stairs landings on section.",
                LongDescription = "Automatically puts dimensions on a current section. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = DimensionStairsLandings.GetPath(),
                IconImageName = "BIM_Leaders_Dimensions_Stairs_Landings.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData07);

            // Populate button data model
            RevitPushButtonDataModel buttonData08 = new RevitPushButtonDataModel
            {
                Label = "Steps\r\nEnumerate",
                Panel = panel,
                ToolTip = "Enumerate Stairs Steps.",
                LongDescription = "Can be useful on section views. Note that only one staircase need to be visible, so check view depth before run.",
                CommandNamespacePath = StairsStepsEnumerate.GetPath(),
                IconImageName = "BIM_Leaders_Stairs_Steps_Enumerate.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSection"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData08);


            panel.AddSeparator();


            // Populate button data model
            RevitPushButtonDataModel buttonData09 = new RevitPushButtonDataModel
            {
                Label = "Align\r\nGrids",
                Panel = panel,
                ToolTip = "Align Grid Ends.",
                LongDescription = "Aligning can be performed only on elevation and section views. Bubbles on ends of the grids can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = GridsAlign.GetPath(),
                IconImageName = "BIM_Leaders_Grids_Align.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsStandard"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData09);

            // Populate button data model
            RevitPushButtonDataModel buttonData10 = new RevitPushButtonDataModel
            {
                Label = "Align\r\nLevels",
                Panel = panel,
                ToolTip = "Align Levels Ends.",
                LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.",
                CommandNamespacePath = LevelsAlign.GetPath(),
                IconImageName = "BIM_Leaders_Levels_Align.png",
                AvailabilityClassName = "BIM_Leaders_Core.ViewIsSectionOrElevation"
            };
            // Create button from provided data
            RevitPushButton.Create(buttonData10);
        }
    }
}
