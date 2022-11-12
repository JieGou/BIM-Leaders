using System;

namespace BIM_Leaders_Logic
{
    internal static class ExceptionUtils
    {
        internal static string GetMessage(Exception exception)
        {
            string message = "";

            message += $"EXCEPTION!{Environment.NewLine}";
            message += $"Message: {exception.Message}{Environment.NewLine}";
            message += $"Method: {exception.TargetSite}{Environment.NewLine}";

            return message;
        }
    }
}