using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;

namespace _BIM_Leaders
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
            RibbonPanel panel_1 = application.CreateRibbonPanel("BIM Leaders", "DWG");

            // Add Button
            PushButtonData button_1_1 = new PushButtonData("button_1_1", "Find DWG", path, "_BIM_Leaders.DWG_View_Found");
            PushButton pushbutton_1_1 = panel_1.AddItem(button_1_1) as PushButton;
            Uri imagePath_1_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_DWG_View_Found.png");
            BitmapImage image_1_1 = new BitmapImage(imagePath_1_1);
            pushbutton_1_1.LargeImage = image_1_1;
            pushbutton_1_1.ToolTip = "Shows information about all DWG files imports";
            pushbutton_1_1.LongDescription = "Shows name of the import, view on which it lays on (if the import is 2D), and import type (Import or Link).";

            // Add Button
            PushButtonData button_1_2 = new PushButtonData("button_1_2", "Delete DWG\r\n by Name", path, "_BIM_Leaders.DWG_Name_Delete");
            PushButton pushbutton_1_2 = panel_1.AddItem(button_1_2) as PushButton;
            Uri imagePath_1_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_DWG_Name_Delete.png");
            BitmapImage image_1_2 = new BitmapImage(imagePath_1_2);
            pushbutton_1_2.LargeImage = image_1_2;
            pushbutton_1_2.ToolTip = "Delete DWG by selected name on all views";
            pushbutton_1_2.LongDescription = "Usable to delete imported DWG, because it can be on many views, and it takes a time to find all those views and delete the DWG manually.";


            // Create Ribbon Panel
            RibbonPanel panel_2 = application.CreateRibbonPanel("BIM Leaders", "Element");

            // Add Button
            PushButtonData button_2_1 = new PushButtonData("button_2_1", "Remove Paint", path, "_BIM_Leaders.Element_Paint_Remove");
            PushButton pushbutton_2_1 = panel_2.AddItem(button_2_1) as PushButton;
            Uri imagePath_2_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Element_Paint_Remove.png");
            BitmapImage image_2_1 = new BitmapImage(imagePath_2_1);
            pushbutton_2_1.LargeImage = image_2_1;
            pushbutton_2_1.ToolTip = "Remove paint from all faces of element";
            pushbutton_2_1.LongDescription = "Usable if need to clear paint from all faces of some element. Usable only for Paint tool";


            // Create Ribbon Panel
            RibbonPanel panel_3 = application.CreateRibbonPanel("BIM Leaders", "Family");

            // Add Button
            PushButtonData button_3_1 = new PushButtonData("button_3_1", "Select\r\nVoids", path, "_BIM_Leaders.Family_Voids_Select");
            button_3_1.AvailabilityClassName = "_BIM_Leaders.DocumentIsFamily";
            PushButton pushbutton_3_1 = panel_3.AddItem(button_3_1) as PushButton;
            Uri imagePath_3_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Familiy_Voids_Select.png");
            BitmapImage image_3_1 = new BitmapImage(imagePath_3_1);
            pushbutton_3_1.LargeImage = image_3_1;
            pushbutton_3_1.ToolTip = "Select voids in a current family";
            pushbutton_3_1.LongDescription = "Usable if need to select void geometry. This is hard if voids not joined with family geometry";

            // Add Button
            PushButtonData button_3_2 = new PushButtonData("button_3_2", "Find\r\nZero", path, "_BIM_Leaders.Family_Zero_Coordinates");
            button_3_2.AvailabilityClassName = "_BIM_Leaders.DocumentIsFamily";
            PushButton pushbutton_3_2 = panel_3.AddItem(button_3_2) as PushButton;
            Uri imagePath_3_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Family_Zero_Coordinates.png");
            BitmapImage image_3_2 = new BitmapImage(imagePath_3_2);
            pushbutton_3_2.LargeImage = image_3_2;
            pushbutton_3_2.ToolTip = "Find Femily Zero";
            pushbutton_3_2.LongDescription = "Creates lines around zero coordinates in a current family. Usable for Profile family and other annotation family types";


            // Create Ribbon Panel
            RibbonPanel panel_4 = application.CreateRibbonPanel("BIM Leaders", "Annotate");

            // Add Button
            PushButtonData button_4_1 = new PushButtonData("button_4_1", "Annotate\r\nSection", path, "_BIM_Leaders.Dimension_Section_Floors");
            button_4_1.AvailabilityClassName = "_BIM_Leaders.ViewIsSection";
            PushButton pushbutton_4_1 = panel_4.AddItem(button_4_1) as PushButton;
            Uri imagePath_4_1 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Dimensions_Section_Floors.png");
            BitmapImage image_4_1 = new BitmapImage(imagePath_4_1);
            pushbutton_4_1.LargeImage = image_4_1;
            pushbutton_4_1.ToolTip = "Dimensions or elevation spots on section";
            pushbutton_4_1.LongDescription = "Automatically puts annotations on a current section. Select a vertical line as a reference for annotations arrangement";
            
            // Add Button
            PushButtonData button_4_2 = new PushButtonData("button_4_2", "Align\r\nGrids", path, "_BIM_Leaders.Grids_Align");
            PushButton pushbutton_4_2 = panel_4.AddItem(button_4_2) as PushButton;
            Uri imagePath_4_2 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Grids_Align.png");
            BitmapImage image_4_2 = new BitmapImage(imagePath_4_2);
            pushbutton_4_2.LargeImage = image_4_2;
            pushbutton_4_2.ToolTip = "Align Grid Ends";
            pushbutton_4_2.LongDescription = "Can be useful on elevation and section views. Bubbles on ends of the grids can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option";

            // Add Button
            PushButtonData button_4_3 = new PushButtonData("button_4_3", "Align\r\nLevels", path, "_BIM_Leaders.Levels_Align");
            PushButton pushbutton_4_3 = panel_4.AddItem(button_4_3) as PushButton;
            Uri imagePath_4_3 = new Uri(@"C:\ProgramData\Autodesk\Revit\Addins\2020\BIM Leaders\BIM_Leaders_Levels_Align.png");
            BitmapImage image_4_3 = new BitmapImage(imagePath_4_3);
            pushbutton_4_3.LargeImage = image_4_3;
            pushbutton_4_3.ToolTip = "Align Levels Ends";
            pushbutton_4_3.LongDescription = "Can be useful on elevation and section views. Tags on ends of the levels can be turned on/off. Internal engine does not understand where is right and left, so if result is not acceptable, try other option";

            return Result.Succeeded;
        }
    }
}
