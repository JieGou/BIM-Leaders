using System;
using Autodesk.Revit.UI;
using BIM_Leaders_Resources;

namespace BIM_Leaders_UI
{
    // Revit pull-down button methods
    public static class RevitPulldownButton
    {
        // Create the push button data provided in <see cref="RevitPushButtonDataModel">
        public static PulldownButton Create(RevitPulldownButtonDataModel data)
        {
            // The button name based on unique ID
            string btnDataName = Guid.NewGuid().ToString();

            // Sets the button data
            PulldownButtonData btnData = new PulldownButtonData(btnDataName, data.Label)
            {
                ToolTip = data.ToolTip,
                LongDescription = data.LongDescription,
                LargeImage = ResourceImage.GetIcon(data.IconImageName)
                //ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName)
            };

            // Return created button and host it on panel provided in required data model
            return data.Panel.AddItem(btnData) as PulldownButton;
        }
    }
}
