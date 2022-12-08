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

        private List<string> _events;
        public List<string> Events
        {
            get { return _events; }
            set
            {
                _events = value;
                OnPropertyChanged(nameof(Events));
            }
        }

        #endregion

        #region METHODS

        private protected override void TryExecute() { }

        public DataSet GetJournalDataSet()
        {
            JournalContent = GetJournalContent();

            Events = new List<string>();

            Events.AddRange(FindCommands());
            Events.AddRange(FindActivate());
            Events.AddRange(FindLButtonUp());

            IEnumerable<ReportMessage> reportMessages = SortEvents();
            DataSet commandsDataSet = GetRunReport(reportMessages);

            if (commandsDataSet.Tables[0].Rows.Count == 0)
                Result.Result = "No commands found in the journal file.";

            return commandsDataSet;
        }

        private string[] GetJournalContent(bool clearFromJunk = true)
        {
            string[] content;

            // Journal Path
            string path = Doc.Application.RecordingJournalFilename;
            string pathNew = path.Substring(0, path.Length - 4) + "TEMP.txt";

            if (File.Exists(pathNew))
                File.Delete(pathNew);

            File.Copy(path, pathNew);

            content = File.ReadAllLines(pathNew);
            if (clearFromJunk)
                content = content
                    .Where( x=> !x.Contains("'"))
                    .Where(x => x.Contains("Jrn."))
                    .ToArray();

            File.Delete(pathNew);

            return content;
        }



        /// <summary>
        /// Find all revit commands in a given string array.
        /// <code>Jrn.Command "Internal" , "Display Profile Dialog , ID_DISPLAY_PROFILE_DIALOG"</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit commands descriptions.</returns>
        private List<string> FindCommands()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.Command"))
                    continue;

                string[] lineDivided0 = line.Trim().Split("\""[0]);
                string[] lineDivided1 = lineDivided0[3].Split(","[0]);

                // "Ribbon", "AccelKey", "KeyboardShortcut", "RepeatLastCommand",
                // "ContextMenu", "Internal", "ProjectBrowser"
                string commandSource = lineDivided0[1];

                string commandDescription = lineDivided1[0].Trim();
                string commandId = lineDivided1[1].Trim();

                events.Add(commandDescription);
            }
            return events;
        }

        /// <summary>
        /// Find all revit events raised when view is activated.
        /// <code>Jrn.Activate "[a43c1afd-7b0f-4696-ab65-30d7fc9e4302.rvt]" , "Floor Plan: -3"</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindActivate()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.Activate"))
                    continue;

                string[] lineDivided0 = line.Trim().Split("\""[0]);
                string[] lineDivided1 = lineDivided0[3].Split(":"[0]);

                string activationDocument = lineDivided0[1].Trim(new char[] {'[', ']'});
                string activationViewType = lineDivided1[0].Trim();
                string activationViewName = lineDivided1[1].Trim();

                events.Add("View Activation");
            }
            return events;
        }

        /// <summary>
        /// Find all revit events raised when view is activated.
        /// <code>Jrn.LButtonDown    1 ,    212 ,    409</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindLButtonUp()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.LButtonUp"))
                    continue;

                string[] lineDivided0 = line.Trim()
                    .Split(new string[] {"    "}, System.StringSplitOptions.None);

                string buttonStatus = lineDivided0[1].Replace(" ,", "");
                string buttonX = lineDivided0[2].Replace(" ,", "");
                string buttonY = lineDivided0[3];

                events.Add("Left Button Up");
            }
            return events;
        }

        /// <summary>
        /// Sort all journaled events and generate data that can be used in reports.
        /// </summary>
        /// <returns>List of ReportMessages</returns>
        private IEnumerable<ReportMessage> SortEvents()
        {
            List<ReportMessage> commandsSorted = new List<ReportMessage>();

            Dictionary<string, int> commands = Events.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());

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

        /*
        Directive,
            Data,
            MouseMove,
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
            TreeCtrl,
            Grid,
            Close

        case Jrn.Data:
                            {
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
                            }
                        case Jrn.Directive:
                            {
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
                            }
                        case Jrn.MouseMove:
                            // Jrn.MouseMove    0 ,    920 ,    742
                            break;
                        case Jrn.LButtonDown:
                            // Jrn.LButtonUp    0 ,    399 ,    462
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
                        case Jrn.Grid:
                            // Jrn.Grid "Control; FormView , Properties , IDD_PROPERTIES_PALETTE; IDC_SYMBOL_GRID" _
                            //          , "Selection" ,  ""
                            break;
                        case Jrn.Close:
                            // Jrn.Close "[a43c1afd-7b0f-4696-ab65-30d7fc9e4302.rvt]" , "Sheet: 000 - ?? ?????"
                            break;

        */
    }
}