using System;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class JournalAnalyze : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // Get Document
            Document doc = uidoc.Document;

            // Journal Path
            string path = doc.Application.RecordingJournalFilename;
            string pathNew = path.Substring(path.Length - 4) + "TEMP.txt";

            try
            {
                File.Copy(path, pathNew);

                string[] content = File.ReadAllLines(pathNew);
                List<string> commands = FindCommands(content);

                Dictionary<string, int> commandsSorted = commands.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

                DataSet commandsDataSet = CreateDataSet(commandsSorted);

                JournalAnalyzeForm form = new JournalAnalyzeForm(commandsDataSet);
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Find all revit commands in a given string array.
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit commands descriptions.</returns>
        private static List<string> FindCommands(string[] content)
        {
            List<string> commands = new List<string>();

            foreach (string line in content)
            {
                // Jrn.Directive
                //      "GlobalToProj"
                //      "ProjToPage"
                //      "ActivateView"
                //      "WindowSize"
                //      "AllowPressAndDrag"
                //      "ScreenResolution"
                //      "DocSymbol"
                //      "IdleTimeTaskSymbol"
                //      "ThinLinesEnabled"
                //      "CategoryDisciplineFilter"
                //      "DisciplineOption"
                //      "TabDisplayOptions"
                //      "Version"
                //      "Username"
                //      "AllowLinkSelection"
                //      "AllowFaceSelection"
                //      "AllowUnderlaySelection"
                //      "AllowPinnedSelection"
                // Jrn.Wheel
                // Jrn.MouseMove
                // Jrn.Data ("Selection action", "ViewManipRotateFlag", "ViewManipZoomFlag", ViewManipPanFlag)
                // Jrn.LButtonUp
                // Jrn.LButtonDown
                // Jrn.MButtonUp
                // Jrn.MButtonDown
                // Jrn.Close

                if (line.Contains("Jrn.Command"))
                {
                    // IDs
                    // string command = "ID_" + line.Split("ID_".ToCharArray())[1].TrimEnd('"'.ToString().ToCharArray());
                    char[] dividers = new[] {"\""[0], ","[0]};
                    string[] commandDivided = line.Split(dividers);

                    string commandSource = commandDivided[1];
                    string commandDescription = commandDivided[4].Trim();
                    string commandId = commandDivided[5].Trim();

                    if (commandSource == "Ribbon" || 
                        commandSource == "AccelKey" || 
                        commandSource == "KeyboardShortcut" || 
                        commandSource == "RepeatLastCommand" ||
                        commandSource == "ContextMenu")
                    {
                        commands.Add(commandDescription);
                    }
                    if (commandSource == "Internal")
                    {
                        //commandDescription = ""                                                                               commandId = "ID_REVIT_MODEL_BROWSER_OPEN";
                        //commandDescription = "Temporary isolate selected elements (in the current view)"                      commandId = "ID_TEMPHIDE_ISOLATE";
                        //commandDescription = "Reset temporary hiding/isolation of elements/categories (in the current view)"  commandId = "ID_TEMPHIDE_ISOLATE";
                        //commandDescription = "Quit the application; prompts to save projects"                                 commandId = "ID_APP_EXIT"
                    }
                    if (commandSource == "ProjectBrowser")
                    {
                        //commandDescription = "Search in Project Browser"  commandId = "ID_PROJECT_BROWSER_SEARCH"
                    }
                }
            }
            return commands;
        }

        /// <summary>
        /// Create a DataSet for using in tables.
        /// </summary>
        /// <returns>Dataset object.</returns>
        private static DataSet CreateDataSet(Dictionary<string, int> commandsSorted)
        {
            // Create a DataSet
            DataSet dataSet = new DataSet("CommandsDataSet");
            // Create DataTable
            DataTable dataTable = new DataTable("Commands");

            // Create 4 columns, and add them to the table
            DataColumn dataColumn0 = new DataColumn("Command", typeof(string));
            DataColumn dataColumn1 = new DataColumn("Count", typeof(string));

            dataTable.Columns.Add(dataColumn0);
            dataTable.Columns.Add(dataColumn1);

            // Add the table to the DataSet
            dataSet.Tables.Add(dataTable);

            // Fill the table
            foreach (KeyValuePair<string, int> i in commandsSorted)
            {
                // Name
                string iCommand = i.Key;
                string iCount = i.Value.ToString();

                DataRow dataRow = dataTable.NewRow();

                dataRow["Command"] = iCommand;
                dataRow["Count"] = iCount;

                // Add the row to the table.  
                dataTable.Rows.Add(dataRow);
            }
            return dataSet;
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(JournalAnalyze).Namespace + "." + nameof(JournalAnalyze);
        }
    }
}