using Autodesk.Revit.UI;

namespace BIM_Leaders_UI
{
    // Represents Revit pull-down button data model
    public class RevitPulldownButtonDataModel
    {
        public string Label { get; set; }
        public RibbonPanel Panel { get; set; }
        public string ToolTip { get; set; }
        public string LongDescription { get; set; }
        public string IconImageName { get; set; }
        public string TooltipImageName { get; set; }

        public RevitPulldownButtonDataModel()
        {

        }
    }
}
