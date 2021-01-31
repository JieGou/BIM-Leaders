using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace _BIM_Leaders
{

    class ExternalDBApp_Group_Restore : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            application.DocumentChanged -= GroupRestoreEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            try
            {
                // Register Event
                application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(GroupRestoreEvent);
            }
            catch(Exception)
            {
                return ExternalDBApplicationResult.Failed;
            }
            return ExternalDBApplicationResult.Succeeded;
        }

        public void GroupRestoreEvent(object sender, DocumentChangedEventArgs args)
        {
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_IOSModelGroups);
            ElementId element = args.GetModifiedElementIds(filter).First();
            string name = args.GetTransactionNames().First();

            if(name == "Restore All Excluded")
            {
                TaskDialog td = new TaskDialog("Group Restoring");
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.Title = "Group Restoring";
                td.MainInstruction = "Group Id " + element.ToString() + " trying to be restored. This is not recommended and can cause problems!";
                td.MainContent = "Contact with model responsible person (Architect, BIM Coordinator) to ensure that groups can be restored.";
                td.Show();
            }  
        }
    }
}
