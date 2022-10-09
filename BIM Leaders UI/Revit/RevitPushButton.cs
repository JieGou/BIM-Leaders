using System;
using Autodesk.Revit.UI;
using BIM_Leaders_Core;
using BIM_Leaders_Resources;

namespace BIM_Leaders_UI
{
    // Revit push button methods
    public static class RevitPushButton
    {
        private static readonly string _assembly = CoreAssembly.GetAssemblyLocation();

        // Create the push button data provided in <see cref="RevitPushButtonDataModel">
        public static PushButton Create(RevitPushButtonDataModel data)
        {
            PushButtonData buttonData = MakePushButtonData(data);

            // Return created button and host it on panel provided in required data model
            return data.Panel.AddItem(buttonData) as PushButton;
        }

        public static PushButton CreateInPulldown(RevitPushButtonDataModel data, PulldownButton button)
        {
            PushButtonData buttonData = MakePushButtonData(data);

            // Return created button and host it on panel provided in required data model
            return button.AddPushButton(buttonData);
        }

        private static PushButtonData MakePushButtonData(RevitPushButtonDataModel data)
        {
            string name = Guid.NewGuid().ToString();

            // Sets the button data
            PushButtonData buttonData = new PushButtonData(name, data.Label, _assembly, data.CommandNamespacePath)
            {
                ToolTip = data.ToolTip,
                LongDescription = data.LongDescription,
                LargeImage = ResourceImage.GetIcon(data.IconImageName),
                AvailabilityClassName = data.AvailabilityClassName,
                //ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName)
            };

            return buttonData;
        }
    }
}
