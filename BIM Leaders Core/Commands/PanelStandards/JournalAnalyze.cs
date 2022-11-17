﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;

namespace BIM_Leaders_Core
{
    [Transaction(TransactionMode.Manual)]
    public class JournalAnalyze : IExternalCommand
    {
        private const string TRANSACTION_NAME = "Analyze Journal";

        private bool _runStarted;
        private bool _runFailed;
        private string _runResult;

        private static Document _doc;
        private string[] _journalContent;
        private DataSet _commandsDataSet;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

            _runStarted = true;

            try
            {
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    _journalContent = GetJournalContent();

                    trans.Commit();
                }

                List<string> commands = FindCommands();
                Dictionary<string, int> commandsSorted = commands.GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
                _commandsDataSet = CreateDataSet(commandsSorted);
                
                ShowResult();

                return Result.Succeeded;
            }
            catch (Exception e)
            {
                _runFailed = true;
                _runResult = e.Message;
                ShowResult();
                return Result.Failed;
            }
        }

        private string[] GetJournalContent()
        {
            string[] content;

            // Journal Path
            string path = _doc.Application.RecordingJournalFilename;
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

            foreach (string line in _journalContent)
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
            DataColumn dataColumn1 = new DataColumn("Count", typeof(int));

            dataTable.Columns.Add(dataColumn0);
            dataTable.Columns.Add(dataColumn1);

            // Add the table to the DataSet
            dataSet.Tables.Add(dataTable);

            // Fill the table
            foreach (KeyValuePair<string, int> i in commandsSorted)
            {
                // Name
                string iCommand = i.Key;
                int iCount = i.Value;

                DataRow dataRow = dataTable.NewRow();

                dataRow["Command"] = iCommand;
                dataRow["Count"] = iCount;

                // Add the row to the table.  
                dataTable.Rows.Add(dataRow);
            }
            return dataSet;
        }

        private void ShowResult()
        {
            if (!_runStarted)
                return;
            if (string.IsNullOrEmpty(_runResult))
                return;

            if (_commandsDataSet.Tables[0].Rows.Count == 0)
            {
                // ViewModel
                ResultVM formVM = new ResultVM(TRANSACTION_NAME, "No commands found in the journal file.");

                // View
                ResultForm form = new ResultForm() { DataContext = formVM };
                form.ShowDialog();
            }
            else
            {
                JournalAnalyzeVM formReportVM = new JournalAnalyzeVM(_commandsDataSet);

                JournalAnalyzeForm formReport = new JournalAnalyzeForm() { DataContext = formReportVM };
                formReport.ShowDialog();
            }
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(JournalAnalyze).Namespace + "." + nameof(JournalAnalyze);
        }
    }
}