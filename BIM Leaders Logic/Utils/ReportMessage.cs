using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    /// <summary>
    /// Message class containing message name and text.
    /// </summary>
    public class ReportMessage
    {
        public string MessageName { get; set; }
        public string MessageText { get; set; }
        public ReportMessage(string messageName, string messageText)
        {
            MessageName = messageName;
            MessageText = messageText;
        }
        public ReportMessage(string messageName)
        {
            MessageName = messageName;
            MessageText = "-";
        }
    }
}