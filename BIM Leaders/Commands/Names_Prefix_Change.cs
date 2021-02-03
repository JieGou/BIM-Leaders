using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace _BIM_Leaders
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Names_Prefix_Change : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            try
            {
                // Getting input data from user
                string prefix_old = "";
                string prefix_new = "";
                List<bool> categories = new List<bool>();
                using (Names_Prefix_Change_Form form = new Names_Prefix_Change_Form())
                {
                    form.ShowDialog();

                    if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        prefix_old = form.Result_prefix_old();
                        prefix_new = form.Result_prefix_new();
                        categories = form.Result_categories();
                    }
                    if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                    {
                        return Result.Cancelled;
                    }
                }

                int count = 0;
                using (Transaction trans = new Transaction(doc, "Names Prefix Change"))
                {
                    trans.Start();
                    /*
                    families = UnwrapElement(IN[0])
                    names = IN[1]
                    for i in range(number):
                        families[i].Name = names[i]
                    */
                    trans.Commit();

                    if (count == 0)
                    {
                        TaskDialog.Show("Names Prefix Change", "No prefixes changed");
                    }
                    else
                    {
                        TaskDialog.Show("DNames Prefix Change", string.Format("{0} prefixes changed", count.ToString()));
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
