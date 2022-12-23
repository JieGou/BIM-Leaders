using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;

namespace BIM_Leaders
{

    class ExternalDBAppGroupRestore : IExternalDBApplication
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
            catch (Exception)
            {
                return ExternalDBApplicationResult.Failed;
            }
            return ExternalDBApplicationResult.Succeeded;
        }

        public void GroupRestoreEvent(object sender, DocumentChangedEventArgs args)
        {
            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_IOSModelGroups);
            ElementId groupId = args.GetModifiedElementIds(filter).First();
            string name = args.GetTransactionNames().First();

            if (name == "Restore All Excluded")
            {
                TaskDialog dialog = new TaskDialog("Group Restoring")
                {
                    MainIcon = TaskDialogIcon.TaskDialogIconWarning,
                    MainInstruction = "Group Id " + groupId.ToString() + " trying to be restored. This is not recommended and can cause problems!",
                    MainContent = "Contact with model responsible person (Architect, BIM Coordinator) to ensure that groups can be restored. If you are not sure, cancel the operation.",
                    CommonButtons = TaskDialogCommonButtons.Ok
                };

                dialog.Show(); 
            }
        }
    }
}