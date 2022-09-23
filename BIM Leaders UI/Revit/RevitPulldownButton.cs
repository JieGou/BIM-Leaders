using System;
using Autodesk.Revit.UI;
using BIM_Leaders_Resources;

namespace BIM_Leaders_UI
{
    // Revit pull-down button methods
    public static class RevitPulldownButton
    {
        private static readonly string _name = Guid.NewGuid().ToString();

        // Create the push button data provided in <see cref="RevitPushButtonDataModel">
        public static PulldownButton Create(RevitPulldownButtonDataModel data)
        {
            PulldownButtonData buttonData = MakePulldownButtonData(data);

            // Return created button and host it on panel provided in required data model
            return data.Panel.AddItem(buttonData) as PulldownButton;
        }

        private static PulldownButtonData MakePulldownButtonData(RevitPulldownButtonDataModel data)
        {
            // Sets the button data
            PulldownButtonData buttonData = new PulldownButtonData(_name, data.Label)
            {
                ToolTip = data.ToolTip,
                LongDescription = data.LongDescription,
                LargeImage = ResourceImage.GetIcon(data.IconImageName)
                //ToolTipImage = ResourceImage.GetIcon(data.TooltipImageName)
            };

            return buttonData;
        }
    }
}
