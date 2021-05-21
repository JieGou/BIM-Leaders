﻿using Autodesk.Revit.UI;

namespace BIM_Leaders_Core
{
    /// <summary>
    ///  Display messages helper methods.
    /// </summary>
    public static class Message
    {
        /// <summary>
        /// Displays the specified message.
        /// </summary>
        /// <param name="message">The message to display in this window</param>
        /// <param name="type">The type of the message <see cref="WindowType"/></param>
        public static void Display(string message, WindowType type)
        {
            string title = "";
            var icon = TaskDialogIcon.TaskDialogIconNone;

            // Customize window based on type of message
            switch (type)
            {
                case WindowType.Information:
                    title = "Information";
                    icon = TaskDialogIcon.TaskDialogIconInformation;
                    break;
                case WindowType.Warning:
                    title = "Warning";
                    icon = TaskDialogIcon.TaskDialogIconWarning;
                    break;
                case WindowType.Error:
                    title = "Error";
                    icon = TaskDialogIcon.TaskDialogIconError;
                    break;
                default:
                    break;
            }

            // Construct window to display specified message
            var window = new TaskDialog(title)
            {
                MainContent = message,
                MainIcon = icon,
                CommonButtons = TaskDialogCommonButtons.Ok

            };
            window.Show();
        }
    }
}
