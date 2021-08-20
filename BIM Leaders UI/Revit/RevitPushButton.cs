using System;
using Autodesk.Revit.UI;
using BIM_Leaders_Core;
using BIM_Leaders_Resources;

namespace BIM_Leaders_UI
{
    // Revit push button methods
    public static class RevitPushButton
    {
        // Create the push button data provided in <see cref="RevitPushButtonDataModel">
        public static PushButton Create(RevitPushButtonDataModel data)
        {
            // The button name based on unique ID
            var btnDataName = Guid.NewGuid().ToString();

            // Sets the button data
            var btnData = new PushButtonData(btnDataName, data.Label, CoreAssembly.GetAssemblyLocation(), data.CommandNamespacePath)
            {
                ToolTip = data.ToolTip,
                LongDescription = data.LongDescription,
                LargeImage = ResourceImage.GetIcon(data.IconImageName),
                //ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName),
                AvailabilityClassName = data.AvailabilityClassName
            };

            // Return created button and host it on panel provided in required data model
            return data.Panel.AddItem(btnData) as PushButton;
        }

        public static PushButton CreateInPulldown(RevitPushButtonDataModel data, PulldownButton button)
        {
            // The button name based on unique ID
            var btnDataName = Guid.NewGuid().ToString();

            // Sets the button data
            var btnData = new PushButtonData(btnDataName, data.Label, CoreAssembly.GetAssemblyLocation(), data.CommandNamespacePath)
            {
                ToolTip = data.ToolTip,
                LongDescription = data.LongDescription,
                LargeImage = ResourceImage.GetIcon(data.IconImageName),
                //ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName),
                AvailabilityClassName = data.AvailabilityClassName
            };

            // Return created button and host it on panel provided in required data model
            return button.AddPushButton(btnData) as PushButton;
        }
    }
}
