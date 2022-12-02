using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
	[Transaction(TransactionMode.Manual)]
    public class JournalAnalyzeModel : BaseModel
    {
        #region PROPERTIES

        private string[] _journalContent;
        public string[] JournalContent
        {
            get { return _journalContent; }
            set
            {
                _journalContent = value;
                OnPropertyChanged(nameof(JournalContent));
            }
        }

        private List<string> _commands;
        public List<string> Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                OnPropertyChanged(nameof(Commands));
            }
        }

        #endregion

        #region METHODS

        private protected override void TryExecute() { }

        public DataSet GetCommandsDataSet()
        {
            JournalContent = GetJournalContent();
            Commands = FindCommands();

            IEnumerable<ReportMessage> reportMessages = SortCommands();
            DataSet commandsDataSet = GetRunReport(reportMessages);

            if (commandsDataSet.Tables[0].Rows.Count == 0)
                Result.Result = "No commands found in the journal file.";

            return commandsDataSet;
        }

        private string[] GetJournalContent()
        {
            string[] content;

            // Journal Path
            string path = Doc.Application.RecordingJournalFilename;
            string pathNew = path.Substring(0, path.Length - 4) + "TEMP.txt";

            if (File.Exists(pathNew))
                File.Delete(pathNew);

            File.Copy(path, pathNew);

            content = File.ReadAllLines(pathNew);

            File.Delete(pathNew);

            return content;
        }

        /// <summary>
        /// Find all revit commands in a given string array.
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit commands descriptions.</returns>
        private List<string> FindCommands()
        {
            List<string> commands = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Contains("'") && line.Contains("Jrn."))
                {
                    string commandEvent = line.Trim().Split(" ".ToCharArray()).First().Remove(0, 4);

                    if (Enum.TryParse(commandEvent, out Jrn commandEventEnum))
                        switch (commandEventEnum)
                        {
                            case Jrn.Command:
                                {
                                    // Synthaxis:
                                    // Jrn.Command "Internal" , "Display Profile Dialog , ID_DISPLAY_PROFILE_DIALOG"

                                    // IDs
                                    // string command = "ID_" + line.Split("ID_".ToCharArray())[1].TrimEnd('"'.ToString().ToCharArray());
                                    char[] dividers = new[] { "\""[0], ","[0] };
                                    string[] commandDivided = line.Trim().Split(dividers);

                                    string commandSource = commandDivided[1];
                                    string commandDescription = commandDivided[4].Trim();
                                    string commandId = commandDivided[5].Trim();

                                    switch (commandSource)
                                    {
                                        case "Ribbon":
                                            commands.Add(commandDescription);
                                            break;
                                        case "AccelKey":
                                            commands.Add(commandDescription);
                                            break;
                                        case "KeyboardShortcut":
                                            commands.Add(commandDescription);
                                            break;
                                        case "RepeatLastCommand":
                                            commands.Add(commandDescription);
                                            break;
                                        case "ContextMenu":
                                            commands.Add(commandDescription);
                                            break;
                                        case "Internal":
                                            break;
                                        case "ProjectBrowser":
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                }
                            case Jrn.Data:
                                //"Selection action"
                                //"ViewManipRotate"
                                //"ViewManipRotateFlag"
                                //"ViewManipZoomFlag"
                                //"ViewManipPanFlag"
                                //"Transaction Successful" !!!USEFUL!!!
                                //"Error dialog"
                                //"Mini warning dialog"
                                //"TaskDialogResult"
                                //"JournalDefaultTemplate"
                                //"JournalDefaultViewDiscipline"
                                //"ExecutedAtomSizeComparison"
                                //"Interrupt"
                                //"Control"
                                //"Restricted Propagation"
                                break;
                            case Jrn.Directive:
                                // Jrn.Directive "ProjToPage"  _

                                //"GlobalToProj"
                                //"ProjToPage"
                                //"ActivateView"
                                //"WindowSize"
                                //"AllowPressAndDrag"
                                //"ScreenResolution"
                                //"DocSymbol"
                                //"IdleTimeTaskSymbol"
                                //"ThinLinesEnabled"
                                //"CategoryDisciplineFilter"
                                //"DisciplineOption"
                                //"TabDisplayOptions"
                                //"Version"
                                //"Username"
                                //"AllowLinkSelection"
                                //"AllowFaceSelection"
                                //"AllowUnderlaySelection"
                                //"AllowPinnedSelection"
                                break;
                            case Jrn.MouseMove:
                                // Jrn.MouseMove    0 ,    920 ,    742
                                break;
                            case Jrn.LButtonUp:
                                // Jrn.LButtonUp    0 ,    399 ,    462
                                break;
                            case Jrn.LButtonDown:
                                // Jrn.LButtonDown    1 ,    212 ,    409
                                break;
                            case Jrn.LButtonDblClk:
                                break;
                            case Jrn.MButtonUp:
                                // Jrn.MButtonUp    0 ,    421 ,    581
                                break;
                            case Jrn.MButtonDown:
                                // Jrn.MButtonDown   16 ,    436 ,    192
                                break;
                            case Jrn.MButtonDblClk:
                                break;
                            case Jrn.Wheel:
                                // Jrn.Wheel      0 ,  120 ,   1215 ,    937
                                break;
                            case Jrn.Key:
                                // Jrn.Key    4 , "VK_SHIFT" , 42
                                break;
                            case Jrn.Size:
                                // Jrn.Size        0 ,   1316 ,    812
                                break;
                            case Jrn.Browser:
                                // Jrn.Browser "LButtonDblClk" _
                                //          , ">>Views (??? ????? ??????)>>BIM TEAM>>IVAN>>118>>Floor Plan: -3>>"
                                break;
                            case Jrn.PropertiesPalette:
                                // Jrn.PropertiesPalette "MouseLeave"
                                break;
                            case Jrn.WidgetEvent:
                                break;
                            case Jrn.RibbonEvent:
                                // Jrn.RibbonEvent "CreateBreadcrumb:"
                                break;
                            case Jrn.AppButtonEvent:
                                break;
                            case Jrn.AddInEvent:
                                break;
                            case Jrn.PushButton:
                                // Jrn.PushButton "ToolBar , {}{} , Dialog_HostObj_SketchEditElevation" _
                                break;
                            case Jrn.RadioButton:
                                // Jrn.RadioButton "ToolBar , {}{} , Dialog_HostObj_WallTopBottom" _
                                break;
                            case Jrn.ComboBox:
                                // Jrn.ComboBox "Modal , New Floor Plan , Dialog_RoomAreaPlan_NewPlanDlg" _
                                //          , "Control_RoomAreaPlan_NewplanTypeDropdown" _
                                //          , "SelEndOk" , "Floor Plan"
                                break;
                            case Jrn.CheckBox:
                                // Jrn.CheckBox "Modal , New Floor Plan , Dialog_RoomAreaPlan_NewPlanDlg" _
                                //          , "Do not duplicate existing views, Control_RoomAreaPlan_NewPlanChkbox" _
                                //          , False
                                break;
                            case Jrn.ListBox:
                                break;
                            case Jrn.Edit:
                                // Jrn.Edit "View , 100003" _
                                //          , "IDC_EDIT_CONTROL" _
                                //          , "ReplaceContents" , "3"
                                break;
                            case Jrn.TreeCtrl:
                                // Jrn.TreeCtrl "Dialog_Family_FamilyHost" , "2110" _
                                //          , "ChangeSelection" , ">>Furniture>>"
                                break;
                            case Jrn.Activate:
                                // Jrn.Activate "[a43c1afd-7b0f-4696-ab65-30d7fc9e4302.rvt]" , "Floor Plan: -3"
                                break;
                            case Jrn.Grid:
                                // Jrn.Grid "Control; FormView , Properties , IDD_PROPERTIES_PALETTE; IDC_SYMBOL_GRID" _
                                //          , "Selection" ,  ""
                                break;
                            case Jrn.Close:
                                // Jrn.Close "[a43c1afd-7b0f-4696-ab65-30d7fc9e4302.rvt]" , "Sheet: 000 - ?? ?????"
                                break;
                            default:
                                break;
                        }
                }
            }
            return commands;
        }

        private enum Jrn
        {
            Directive,
            Data,
            MouseMove,
            LButtonUp,
            LButtonDown,
            LButtonDblClk,
            MButtonUp,
            MButtonDown,
            MButtonDblClk,
            Wheel,
            Key,
            Size,
            Browser,
            PropertiesPalette,
            WidgetEvent,
            RibbonEvent,
            AppButtonEvent,
            AddInEvent,
            PushButton,
            RadioButton,
            ComboBox,
            CheckBox,
            ListBox,
            Edit,
            Command,
            TreeCtrl,
            Activate,
            Grid,
            Close
        }

        /// <summary>
        /// Find all revit commands in a given string array.
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit commands descriptions.</returns>
        private static List<string> FindJrn(string[] content)
        {
            List<string> commands = new List<string>();

            foreach (string line in content)
                if (!line.Contains("'") && line.Contains("Jrn."))
                {
                    string command = line.Trim().Split(" ".ToCharArray()).First();
                    if (!commands.Contains(command))
                        commands.Add(command);
                }

            return commands;
        }

        private IEnumerable<ReportMessage> SortCommands()
        {
            List<ReportMessage> commandsSorted = new List<ReportMessage>();

            Dictionary<string, int> commands = Commands.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

            foreach (KeyValuePair<string, int> command in commands)
            {
                ReportMessage message = new ReportMessage(command.Key, command.Value);
                commandsSorted.Add(message);
            }

            return commandsSorted;
        }

        /// <summary>
        /// Create a DataSet for using in tables.
        /// </summary>
        /// <returns>Dataset object.</returns>
        private protected override DataSet GetRunReport(IEnumerable<ReportMessage> reportMessages)
        {
            // Create a DataSet
            DataSet dataSet = new DataSet("reportDataSet");
            // Create DataTable
            DataTable dataTable = new DataTable("Commands");

            // Create 4 columns, and add them to the table
            DataColumn dataColumnCommand = new DataColumn("Command", typeof(string));
            DataColumn dataColumnCount = new DataColumn("Count", typeof(int));

            dataTable.Columns.Add(dataColumnCommand);
            dataTable.Columns.Add(dataColumnCount);

            // Add the table to the DataSet
            dataSet.Tables.Add(dataTable);

            // Fill the table
            foreach (ReportMessage reportMessage in reportMessages)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["Command"] = reportMessage.MessageName;
                dataRow["Count"] = reportMessage.MessageCount;
                dataTable.Rows.Add(dataRow);
            }

            return dataSet;
        }

        #endregion
    }
}