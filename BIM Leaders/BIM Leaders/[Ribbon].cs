using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using BIM_Leaders_UI;

namespace BIM_Leaders_Core
{
    // Get true if in family document
    public class DocumentIsFamily : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                Document o_doc = applicationData.ActiveUIDocument.Document;

                if (o_doc.IsFamilyDocument)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
    // Get true if in section view
    public class ViewIsSection : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType v_type = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (v_type == ViewType.Section)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
    // Get true if in plan view
    public class ViewIsPlan : IExternalCommandAvailability
    {
        public static bool IsCommandAvaiable { get; internal set; }

        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            try
            {
                ViewType v_type = applicationData.ActiveUIDocument.Document.ActiveView.ViewType;

                if (v_type == ViewType.FloorPlan)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
    class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
        public Result OnStartup(UIControlledApplication application)
        {
            // Create Ribbon Tab
            application.CreateRibbonTab("BIM Leaders");
            string path = Assembly.GetExecutingAssembly().Location;

            // Create Ribbon Panel
            RibbonPanel panel_1 = application.CreateRibbonPanel("BIM Leaders", "Standards");

            // Add Button
            PushButtonData button_1_1 = new PushButtonData("button_1_1", "Names Prefix\r\nChange", path, "_BIM_Leaders.Names_Prefix_Change");
            PushButton pushbutton_1_1 = panel_1.AddItem(button_1_1) as PushButton;
            Uri imagePath_1_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Names_Prefix_Change.png");
            BitmapImage image_1_1 = new BitmapImage(imagePath_1_1);
            pushbutton_1_1.LargeImage = image_1_1;
            pushbutton_1_1.ToolTip = "Change all names prefixes.";
            pushbutton_1_1.LongDescription = "Check which categories need to be renamed. Also can be useful to change part of the names in the middle or at the end.";

            // Add Button
            PushButtonData button_1_2 = new PushButtonData("button_1_2", "Delete Unused\r\nLinestyles", path, "_BIM_Leaders.Linestyles_Unused_Delete");
            PushButton pushbutton_1_2 = panel_1.AddItem(button_1_2) as PushButton;
            Uri imagePath_1_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Linestyles_Unused_Delete.png");
            BitmapImage image_1_2 = new BitmapImage(imagePath_1_2);
            pushbutton_1_2.LargeImage = image_1_2;
            pushbutton_1_2.ToolTip = "Delete all unused linestyles.";
            pushbutton_1_2.LongDescription = "Linestyles cannot be deleted via Purge function. Use this command to purge all linestyles that are unneeded.";

            // Add Button
            PushButtonData button_1_3 = new PushButtonData("button_1_3", "Delete IMPORT\r\nLinetypes", path, "_BIM_Leaders.Linetypes_IMPORT_Delete");
            PushButton pushbutton_1_3 = panel_1.AddItem(button_1_3) as PushButton;
            Uri imagePath_1_3 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Linetypes_IMPORT_Delete.png");
            BitmapImage image_1_3 = new BitmapImage(imagePath_1_3);
            pushbutton_1_3.LargeImage = image_1_3;
            pushbutton_1_3.ToolTip = "Delete all linetypes of the given name.";
            pushbutton_1_3.LongDescription = "Can be useful to delete IMPORT linetypes after importing DWG files.";

            // Add Button
            PushButtonData button_1_4 = new PushButtonData("button_1_4", "Walls\r\nParralel", path, "_BIM_Leaders.Walls_Parallel");
            button_1_4.AvailabilityClassName = "_BIM_Leaders.ViewIsPlan";
            PushButton pushbutton_1_4 = panel_1.AddItem(button_1_4) as PushButton;
            Uri imagePath_1_4 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Walls_Parallel.png");
            BitmapImage image_1_4 = new BitmapImage(imagePath_1_4);
            pushbutton_1_4.LargeImage = image_1_4;
            pushbutton_1_4.ToolTip = "Walls parallel check.";
            pushbutton_1_4.LongDescription = "Creates wall checking filter. All non-parallel and non-perpendicular walls will be colored.";

            // Create Ribbon Panel
            RibbonPanel panel_2 = application.CreateRibbonPanel("BIM Leaders", "DWG");

            // Add Button
            PushButtonData button_2_1 = new PushButtonData("button_2_1", "Find DWG", path, "_BIM_Leaders.DWG_View_Found");
            PushButton pushbutton_2_1 = panel_2.AddItem(button_2_1) as PushButton;
            Uri imagePath_2_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_DWG_View_Found.png");
            BitmapImage image_2_1 = new BitmapImage(imagePath_2_1);
            pushbutton_2_1.LargeImage = image_2_1;
            pushbutton_2_1.ToolTip = "Shows information about all DWG files imports.";
            pushbutton_2_1.LongDescription = "Shows name of the import, view on which it lays on (if the import is 2D), and import type (Import or Link).";

            // Add Button
            PushButtonData button_2_2 = new PushButtonData("button_2_2", "Delete DWG\r\nby Name", path, "_BIM_Leaders.DWG_Name_Delete");
            PushButton pushbutton_2_2 = panel_2.AddItem(button_2_2) as PushButton;
            Uri imagePath_2_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_DWG_Name_Delete.png");
            BitmapImage image_2_2 = new BitmapImage(imagePath_2_2);
            pushbutton_2_2.LargeImage = image_2_2;
            pushbutton_2_2.ToolTip = "Delete DWG by selected name on all views.";
            pushbutton_2_2.LongDescription = "Usable to delete imported DWG, because it can be on many views, and it takes a time to find all those views and delete the DWG manually.";


            // Create Ribbon Panel
            RibbonPanel panel_3 = application.CreateRibbonPanel("BIM Leaders", "Element");

            // Add Button
            PushButtonData button_3_1 = new PushButtonData("button_3_1", "Remove Paint", path, "_BIM_Leaders.Element_Paint_Remove");
            PushButton pushbutton_3_1 = panel_3.AddItem(button_3_1) as PushButton;
            Uri imagePath_3_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Element_Paint_Remove.png");
            BitmapImage image_3_1 = new BitmapImage(imagePath_3_1);
            pushbutton_3_1.LargeImage = image_3_1;
            pushbutton_3_1.ToolTip = "Remove paint from all faces of element.";
            pushbutton_3_1.LongDescription = "Usable if need to clear paint from all faces of some element. Usable only for Paint tool.";


            // Create Ribbon Panel
            RibbonPanel panel_4 = application.CreateRibbonPanel("BIM Leaders", "Family");

            // Add Button
            PushButtonData button_4_1 = new PushButtonData("button_4_1", "Select\r\nVoids", path, "_BIM_Leaders.Family_Voids_Select");
            button_4_1.AvailabilityClassName = "_BIM_Leaders.DocumentIsFamily";
            PushButton pushbutton_4_1 = panel_4.AddItem(button_4_1) as PushButton;
            Uri imagePath_4_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Familiy_Voids_Select.png");
            BitmapImage image_4_1 = new BitmapImage(imagePath_4_1);
            pushbutton_4_1.LargeImage = image_4_1;
            pushbutton_4_1.ToolTip = "Select voids in a current family.";
            pushbutton_4_1.LongDescription = "Usable if need to select void geometry. This is hard if voids not joined with family geometry.";

            // Add Button
            PushButtonData button_4_2 = new PushButtonData("button_4_2", "Find\r\nZero", path, "_BIM_Leaders.Family_Zero_Coordinates");
            button_4_2.AvailabilityClassName = "_BIM_Leaders.DocumentIsFamily";
            PushButton pushbutton_4_2 = panel_4.AddItem(button_4_2) as PushButton;
            Uri imagePath_4_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Family_Zero_Coordinates.png");
            BitmapImage image_4_2 = new BitmapImage(imagePath_4_2);
            pushbutton_4_2.LargeImage = image_4_2;
            pushbutton_4_2.ToolTip = "Find Femily Zero.";
            pushbutton_4_2.LongDescription = "Creates lines around zero coordinates in a current family. Usable for Profile family and other annotation family types.";


            // Create Ribbon Panel
            RibbonPanel panel_5 = application.CreateRibbonPanel("BIM Leaders", "Annotate");

            // Add Button
            PushButtonData button_5_1 = new PushButtonData("button_5_1", "Annotate\r\nSection", path, "_BIM_Leaders.Dimension_Section_Floors");
            button_5_1.AvailabilityClassName = "_BIM_Leaders.ViewIsSection";
            PushButton pushbutton_5_1 = panel_5.AddItem(button_5_1) as PushButton;
            Uri imagePath_5_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Dimensions_Section_Floors.png");
            BitmapImage image_5_1 = new BitmapImage(imagePath_5_1);
            pushbutton_5_1.LargeImage = image_5_1;
            pushbutton_5_1.ToolTip = "Dimensions or elevation spots on section.";
            pushbutton_5_1.LongDescription = "Automatically puts annotations on a current section. Select a vertical line as a reference for annotations arrangement.";

            // Add Button
            PushButtonData button_5_2 = new PushButtonData("button_5_2", "Annotate\r\nLandings", path, "_BIM_Leaders.Dimension_Stairs_Landings");
            button_5_2.AvailabilityClassName = "_BIM_Leaders.ViewIsSection";
            PushButton pushbutton_5_2 = panel_5.AddItem(button_5_2) as PushButton;
            Uri imagePath_5_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Dimensions_Stairs_Landings.png");
            BitmapImage image_5_2 = new BitmapImage(imagePath_5_2);
            pushbutton_5_2.LargeImage = image_5_2;
            pushbutton_5_2.ToolTip = "Dimensions stairs landings on section.";
            pushbutton_5_2.LongDescription = "Automatically puts dimensions on a current section. Note that only one staircase need to be visible, so check view depth before run.";

            // Add Button
            PushButtonData button_5_3 = new PushButtonData("button_5_3", "Align\r\nGrids", path, "_BIM_Leaders.Grids_Align");
            PushButton pushbutton_5_3 = panel_5.AddItem(button_5_3) as PushButton;
            Uri imagePath_5_3 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Grids_Align.png");
            BitmapImage image_5_3 = new BitmapImage(imagePath_5_3);
            pushbutton_5_3.LargeImage = image_5_3;
            pushbutton_5_3.ToolTip = "Align Grid Ends.";
            pushbutton_5_3.LongDescription = "Can be useful on elevation and section views. Bubbles on ends of the grids can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.";

            // Add Button
            PushButtonData button_5_4 = new PushButtonData("button_5_4", "Align\r\nLevels", path, "_BIM_Leaders.Levels_Align");
            PushButton pushbutton_5_4 = panel_5.AddItem(button_5_4) as PushButton;
            Uri imagePath_5_4 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Levels_Align.png");
            BitmapImage image_5_4 = new BitmapImage(imagePath_5_4);
            pushbutton_5_4.LargeImage = image_5_4;
            pushbutton_5_4.ToolTip = "Align Levels Ends.";
            pushbutton_5_4.LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option.";

            // Add Button
            PushButtonData button_5_5 = new PushButtonData("button_5_5", "Steps\r\nEnumerate", path, "_BIM_Leaders.Stairs_Steps_Enumerate");
            button_5_5.AvailabilityClassName = "_BIM_Leaders.ViewIsSection";
            PushButton pushbutton_5_5 = panel_5.AddItem(button_5_5) as PushButton;
            Uri imagePath_5_5 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Stairs_Steps_Enumerate.png");
            BitmapImage image_5_5 = new BitmapImage(imagePath_5_5);
            pushbutton_5_5.LargeImage = image_5_5;
            pushbutton_5_5.ToolTip = "Enumerate Stairs Steps.";
            pushbutton_5_5.LongDescription = "Can be useful on section views. Note that only one staircase need to be visible, so check view depth before run.";

            // Add Button
            PushButtonData button_5_6 = new PushButtonData("button_5_6", "Walls\r\nCompare", path, "_BIM_Leaders.Walls_Compare");
            button_5_6.AvailabilityClassName = "_BIM_Leaders.ViewIsPlan";
            PushButton pushbutton_5_6 = panel_5.AddItem(button_5_6) as PushButton;
            Uri imagePath_5_6 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Walls_Compare.png");
            BitmapImage image_5_6 = new BitmapImage(imagePath_5_6);
            pushbutton_5_6.LargeImage = image_5_6;
            pushbutton_5_6.ToolTip = "Compare Walls for Permit Change Drawing.";
            pushbutton_5_6.LongDescription = "Can be useful on plan views. Select a Revit link after settings entering.";

            return Result.Succeeded;
        }
    }
}
