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

        public bool AnalyzeCommands { get; set; }
        public bool AnalyzeActivate { get; set; }
        public bool AnalyzeWheel { get; set; }
        public bool AnalyzeMouseButtons { get; set; }
        public bool AnalyzeMouseMove { get; set; }
        public bool AnalyzeKey { get; set; }

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

        private protected override void TryExecute()
        {
            JournalContent = GetJournalContent();

            Events = new List<string>();

            if (AnalyzeCommands)
                Events.AddRange(FindCommands());
            if (AnalyzeActivate)
                Events.AddRange(FindActivate());
            if (AnalyzeWheel)
                Events.AddRange(FindWheel());
            if (AnalyzeMouseButtons)
                Events.AddRange(FindMouseButtons());
            if (AnalyzeMouseMove)
                Events.AddRange(FindMouseMove());
            if (AnalyzeKey)
                Events.AddRange(FindKey());

            IEnumerable<ReportMessage> reportMessages = SortEvents();

            if (reportMessages.Count() == 0)
            {
                Result.Result = "No commands found in the journal file.";
                return;
            }

            Result.Report = GetRunReport(reportMessages);
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

                char[] dividers = { '\"', ',' };
                string[] lineDivided = line.Split(dividers, StringSplitOptions.RemoveEmptyEntries);

                // "Ribbon", "AccelKey", "KeyboardShortcut", "RepeatLastCommand",
                // "ContextMenu", "Internal", "ProjectBrowser" "PrintPreviewUI",
                // "StartupPage", "StatusBar", "SystemMenu"
                string commandSource = lineDivided[1];

                string commandDescription = lineDivided[4].Trim();
                string commandId = lineDivided[5].Trim();

                if (!string.IsNullOrEmpty(commandDescription))
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

        // buttonStatus
        //
        // Buttons status are used in Jrn.XButtonXX, Jrn.MouseMove, Jrn.Key
        // Shows the status of all buttons that pressed at the moment
        //
        // Saves into two summary values:
        // Jrn.Key       - summary value of keyboard buttons
        // Jrn.XButtonXX - summary value of all buttons (keyboard + mouse)
        // Jrn.MouseMove - summary value of all buttons (keyboard + mouse)

        /// <summary>
        /// Find all revit events raised when mouse wheel is rotated.
        /// <code>Jrn.Wheel      0 ,  120 ,    982 ,    658</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindWheel()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.Wheel"))
                    continue;

                events.Add("Wheel");
                /*
                char[] dividers = { ' ' };
                string[] lineDivided0 = line.Split(dividers, StringSplitOptions.RemoveEmptyEntries);

                // Always 0
                string zero = lineDivided0[1].Trim();

                // Zooming out or in
                // -720 -360 -240 -120 120 240 360 720
                string zoom = lineDivided0[3].Trim();

                // 750
                string x = lineDivided0[5].Trim();

                // 436
                string y = lineDivided0[7].Trim();
                */
            }
            return events;
        }

        /// <summary>
        /// Find all revit events raised when mouse button is pressed.
        /// <code>Jrn.LButtonDown    1 ,    212 ,    409</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindMouseButtons()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (line.Trim().StartsWith("Jrn.LButtonDown"))
                    events.Add("Left Button Down");
                //else if(line.Trim().StartsWith("Jrn.LButtonUp"))
                //    events.Add("Left Button Up");
                else if (line.Trim().StartsWith("Jrn.LButtonDblClk"))
                    events.Add("Left Button Double Click");
                else if (line.Trim().StartsWith("Jrn.RButtonDown"))
                    events.Add("Right Button Down");
                //else if (line.Trim().StartsWith("Jrn.RButtonUp"))
                //    events.Add("Right Button Up");
                else if (line.Trim().StartsWith("Jrn.RButtonDblClk"))
                    events.Add("Right Button Double Click");
                else if (line.Trim().StartsWith("Jrn.MButtonDown"))
                    events.Add("Middle Button Down");
                //else if (line.Trim().StartsWith("Jrn.MButtonUp"))
                //    events.Add("Middle Button Up");
                else if (line.Trim().StartsWith("Jrn.MButtonDblClk"))
                    events.Add("Middle Button Double Click");
                /*
                char[] dividers = { ' ' };
                string[] lineDivided0 = line.Split(dividers, StringSplitOptions.RemoveEmptyEntries);

                // Clear values (real values can be combinated):
                //
                // Jrn.LButtonUp = 0
                // Jrn.LButtonDown = 1
                // Jrn.LButtonDblClk = 1
                // Jrn.RButtonUp = 0
                // Jrn.RButtonDown = 2
                // Jrn.RButtonDblClk = ?
                // Jrn.MButtonUp = 0
                // Jrn.MButtonDown = 16
                // Jrn.MButtonDblClk = 16

                string buttonStatus = lineDivided0[1].Trim().Replace(" ,", "");

                string buttonX = lineDivided0[2].Replace(" ,", "");
                string buttonY = lineDivided0[3];
                */
            }
            return events;
        }

        /// <summary>
        /// Find all revit events raised when mouse is moved.
        /// <code>Jrn.MouseMove    0 ,    212 ,    409</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindMouseMove()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.MouseMove"))
                    continue;

                events.Add("Mouse Move");
                /*
                char[] dividers = { ' ' };
                string[] lineDivided0 = line.Split(dividers, StringSplitOptions.RemoveEmptyEntries);

                // Number appears after same number in key/button pressing event:
                //
                // 0  - No
                // 1  - LButtonDown
                // 4  - LButtonUp | MButtonUp | Key
                // 8  - Key
                // 16 - MButtonDown
                //
                // Seen also combinated values:
                //
                // 5  - Key + LButtonDown
                // 9  - Key + LButtonDown
                // 20 - Key + MButtonDown
                // 24 - Key + MButtonDown
                //
                string buttonStatus = lineDivided0[1].Trim().Replace(" ,", "");

                string buttonX = lineDivided0[2].Replace(" ,", "");
                string buttonY = lineDivided0[3];
                */
            }
            return events;
        }

        /// <summary>
        /// Find all revit events raised when key is pressed.
        /// <code>Jrn.Key    4 , "VK_SHIFT" , 42</code>
        /// </summary>
        /// <param name="content">String array.</param>
        /// <returns>List of strings that contains revit events descriptions.</returns>
        private List<string> FindKey()
        {
            List<string> events = new List<string>();

            foreach (string line in JournalContent)
            {
                if (!line.Trim().StartsWith("Jrn.Key"))
                    continue;
                
                events.Add("Key pressed/unpressed");

                char[] dividers = { ' ' };
                string[] lineDivided0 = line.Split(dividers, StringSplitOptions.RemoveEmptyEntries);

                // Number appears after same number in key pressing event:
                //
                // 0  - VK_UP | VK_DOWN | VK_F1 | VK_F5 | Tab | CR | 2 | 5 | 6
                // 4  - VK_SHIFT
                // 8  - VK_CONTROL
                //
                // Seen also combinated values:
                //
                // 12  - VK_SHIFT + VK_CONTROL
                //
                string buttonStatus = lineDivided0[1].Trim();

                // VK_F1 | VK_F5 | Tab | CR | 2 | 5 | 6 | VK_CONTROL | VK_SHIFT | VK_UP | VK_DOWN
                string keyId = lineDivided0[3].Replace(" ,", "");

                // 0     - VK_F1 | VK_F5 | Tab | CR | 2 | 5 | 6
                // 29    - VK_CONTROL   (pressed)
                // 42    - VK_SHIFT     (pressed)
                // 328   - VK_UP        (pressed)
                // 336   - VK_DOWN      (pressed)
                // 32797 - VK_CONTROL          ()
                // 49181 - VK_CONTROL (unpressed)
                // 49194 - VK_SHIFT   (unpressed)
                // 49480 - VK_UP      (unpressed)
                // 49488 - VK_DOWN    (unpressed)
                string keyCode = lineDivided0[5];            
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
        
        COMMENTS

        Timastamps

        Types: C | E | H
        'H 05-Dec-2022 12:15:22.744;   0:< 

        */


        /*
        --------------------
        UI Events

        Activate
        AppButtonEvent
        Browser
        Checkbox
        Close
        ComboBox
        DropFiles
        Edit
        Grid
        ListBox
        Maximize
        Minimize
        PropertiesPalette
        PushButton
        RadioButton
        Restore
        RibbonEvent
        SBTrayAction
        Size
        SliderCtrl
        TabCtrl
        TreeCtrl
        WidgetEvent
        -------------------

        Jrn.Size:
        // Jrn.Size        0 ,   1316 ,    812
        // Jrn.Size        0 ,   1268 ,    812
        // Jrn.Size        0 ,   1276 ,    820
        
        Jrn.Browser:
        // Jrn.Browser "LButtonDblClk" _
        //          , ">>Views (??? ????? ??????)>>BIM TEAM>>IVAN>>118>>Floor Plan: -3>>"

        Jrn.PropertiesPalette:
        // Jrn.PropertiesPalette "MouseLeave"

        Jrn.WidgetEvent:

        Jrn.RibbonEvent:
        // Jrn.RibbonEvent "CreateBreadcrumb:"
        // Jrn.RibbonEvent "ComboCurrentChangedEvent:TypeSelector:NO TITLE:Viewport"
        // Jrn.RibbonEvent "TabActivated:Architecture"
        // Jrn.RibbonEvent "TabActivated:Modify"
        // Jrn.RibbonEvent "ModelBrowserIsOpenChangedEvent:Open"
        // Jrn.RibbonEvent "ModelBrowserOpenDocumentEvent:open:{""projectGuid"":""8b5319e9-dbad-44cc-a95c-ca8a059bd7f6"",""modelGuid"":""789b4b04-7f4b-44e1-8747-653880715f4f"",""id"":""cld://{8b5319e9-dbad-44cc-a95c-ca8a059bd7f6}Netanya%20Site1409%20(R20)/{789b4b04-7f4b-44e1-8747-653880715f4f}A-1409-MAIN.rvt"",""displayName"":""A-1409-MAIN""}"
        // Jrn.RibbonEvent "ModelBrowserIsOpenChangedEvent:Close"
        
        Jrn.AppButtonEvent:
        // Jrn.AppButtonEvent 1 , "Application Menu is opening"
        // Jrn.AppButtonEvent 0 , "Application Menu is closing"

        Jrn.PushButton:
        // Jrn.PushButton "FormView , Properties , IDD_PROPERTIES_PALETTE" _
        //          , "Edit Type, ID_EDIT_SYMBOL"
        // Jrn.PushButton "Modal , Visibility/Graphic Overrides for Floor Plan: 25 , 0" _
        //         , "OK, IDOK"
        // Jrn.PushButton "Modeless , Autodesk Revit 2020 , Dialog_Revit_DocWarnDialog" _
        //          , "Adjust Limits..., IDRETRY"
        // Jrn.PushButton "ToolBar , {}{} , Dialog_HostObj_SketchEditElevation" _
        
        Jrn.RadioButton:
        // Jrn.RadioButton "ToolBar , {}{} , Dialog_HostObj_WallTopBottom" _
        
        Jrn.ComboBox:
        // Jrn.ComboBox "Modal , New Floor Plan , Dialog_RoomAreaPlan_NewPlanDlg" _
        //          , "Control_RoomAreaPlan_NewplanTypeDropdown" _
        //          , "SelEndOk" , "Floor Plan"
        
        Jrn.CheckBox:
        // Jrn.CheckBox "Modal , New Floor Plan , Dialog_RoomAreaPlan_NewPlanDlg" _
        //          , "Do not duplicate existing views, Control_RoomAreaPlan_NewPlanChkbox" _
        //          , False
        // Jrn.CheckBox "ToolBar , {}{} , Dialog_Essentials_FilterSelectionDo" _
        //          , "Active Option Only, Control_Essentials_ActiveoptionOnly" _
        //          , False
        
        Jrn.ListBox:
        // Jrn.ListBox "Modal , Assign View Template , Dialog_Revit_ViewTemplates" _
        //        , "Control_Revit_ViewsList" _
        //        , "Select" , "Hagasha_Plans_Area_100"

        Jrn.Edit:
        // Jrn.Edit "View , 100003" _
        //          , "IDC_EDIT_CONTROL" _
        //          , "ReplaceContents" , "3"
        
        Jrn.TreeCtrl:
        // Jrn.TreeCtrl "Dialog_Family_FamilyHost" , "2110" _
        //          , "ChangeSelection" , ">>Furniture>>"
        
        Jrn.Grid:
        // Jrn.Grid "Control; FormView , Properties , IDD_PROPERTIES_PALETTE; IDC_SYMBOL_GRID" _
        //          , "Selection" ,  ""
        
        Jrn.Close:
        // Jrn.Close "[a43c1afd-7b0f-4696-ab65-30d7fc9e4302.rvt]" , "Sheet: 000 - ?? ?????"

        ------------
        Other Events
        
        Data,
        Directive,
        AddInEvent,
        Scroll,
        ------------

        Jrn.Data
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

        Jrn.Directive:
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
                            
        Jrn.AddInEvent:
        // Jrn.AddInEvent "AddInJournaling" , "WpfPopup(PART_Popup).WpfListBox(0,mComboMenuListBox).SelectItems(0,UIFramework\.RecentTypesCtrlItem)"

        */
    }
}