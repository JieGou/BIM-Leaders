using System;
using System.Collections.Generic;
using System.Linq;

namespace BIM_Leaders_Logic
{
    #region JOURNAL LINE CLASSES

    public class JournalLine
    {
        public int Number { get; set; }
        public string RawText { get; set; }
        public int Block { get; set; }
        public Journal Journal { get; set; }

        public JournalLine(int number, string raw, int block)
        {
            Number = number;
            RawText = raw;
            Block = block;
        }

        public JournalLine Previuos()
        {
            int index = Journal.Lines.IndexOf(this);
            if (index > 0)
                return Journal.Lines[index - 1];

            return null;
        }

        public JournalLine PreviuosOfType(Type type) => AllPreviuosOfType(type).LastOrDefault();

        public List<JournalLine> AllPreviuos()
        {
            List<JournalLine> result = new List<JournalLine>();

            int index = Journal.Lines.IndexOf(this);

            for (int i = 0; i < index; i++)
            {
                result.Add(Journal.Lines[i]);
            }

            return result;
        }

        public List<JournalLine> AllPreviuosOfType(Type type)
        {
            List<JournalLine> result = new List<JournalLine>();

            int index = Journal.Lines.IndexOf(this);

            for (int i = 0; i < index; i++)
            {
                if (Journal.Lines[i].GetType() == type)
                    result.Add(Journal.Lines[i]);
            }

            return result;
        }

        public JournalLine Next()
        {
            int index = Journal.Lines.IndexOf(this);
            if (index + 1 < Journal.LineCount)
                return Journal.Lines[index + 1];

            return null;
        }

        public JournalLine NextOfType(Type type) => AllNextOfType(type).FirstOrDefault();

        public List<JournalLine> AllNext()
        {
            List<JournalLine> result = new List<JournalLine>();

            int index = Journal.Lines.IndexOf(this);

            for (int i = index; i < Journal.LineCount; i++)
            {
                result.Add(Journal.Lines[i]);
            }

            return result;
        }

        public List<JournalLine> AllNextOfType(Type type)
        {
            List<JournalLine> result = new List<JournalLine>();

            int index = Journal.Lines.IndexOf(this);

            for (int i = index; i < Journal.LineCount; i++)
            {
                if (Journal.Lines[i].GetType() == type)
                    result.Add(Journal.Lines[i]);
            }

            return result;
        }

        public DateTime GetDateTimeOfBlock() => Journal.GetDateTimeByBlock(Block);
    }

    public class JournalLineAddinEvent : JournalLine
    {
        public string MessageText { get; set; }

        public JournalLineAddinEvent(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    /// <summary>
    /// ' 0:< API_SUCCESS { Starting External DB Application: ConnectedDesktopDB, Class: ConnectedDesktopDB.ServerApp, Vendor : ADSK(Autodesk, www.autodesk.com), Assembly: C:\Program Files\Autodesk\Revit 2020\AddIns\ConnectedDesktop\ConnectedDesktopDB.dll } 
    /// </summary>
    public class JournalLineAPIMessage : JournalLine
    {
        public bool IsError { get; set; }
        public string MessageText { get; set; }
        public string MessageType { get; set; }

        public JournalLineAPIMessage(int number, string raw, int block, bool isError)
            : base(number, raw, block)
        {
            IsError = isError;
        }
    }

    public class JournalLineBasicFileInfo : JournalLine
    {
        public bool Worksharing { get; set; }
        public string CentralModelPath { get; set; }
        public string LastSavePath { get; set; }
        public string Locale { get; set; }
        public string FileName { get; set; }

        public JournalLineBasicFileInfo(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineCommand : JournalLine
    {
        public string CommandType { get; set; }
        public string CommandDescription { get; set; }
        public string CommandId { get; set; }

        public JournalLineCommand(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineComment : JournalLine
    {
        public JournalLineComment(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineData : JournalLine
    {
        public string Key { get; set; }
        public string[] Values { get; set; }

        public JournalLineData(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineDirective : JournalLine
    {
        public string Key { get; set; }
        public string[] Values { get; set; }

        public JournalLineDirective(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineGUIResourceUsage : JournalLine
    {
        public bool Available { get; set; }
        public bool Used { get; set; }
        public string User { get; set; }

        public JournalLineGUIResourceUsage(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineKeyboardEvent : JournalLine
    {
        public char Key { get; set; }

        public JournalLineKeyboardEvent(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineMemoryMetrics : JournalLine
    {
        public int VMAvailable { get; set; }
        public int VMUsed { get; set; }
        public int VMPeak { get; set; }
        public int RAMAvailable { get; set; }
        public int RAMUsed { get; set; }
        public int RAMPeak { get; set; }

        public JournalLineMemoryMetrics(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    public class JournalLineMiscCommand : JournalLine
    {
        public JournalLineMiscCommand(int number, string raw, int block, bool isError)
            : base(number, raw, block) { }
    }

    public class JournalLineMouseEvent : JournalLine
    {
        public string MouseEventType { get; set; }
        public string[] Data { get; set; }

        public JournalLineMouseEvent(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    public class JournalLineSystemInformation : JournalLine
    {
        public string SystemInformationType { get; set; }
        public int ItemNumber { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public JournalLineSystemInformation(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    public class JournalLineTimeStamp : JournalLine
    {
        public string TimeStampType { get; set; }
        public DateTime DateTime { get; set; }
        public string Description { get; set; }

        public JournalLineTimeStamp(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    public class JournalLineUIEvent : JournalLine
    {
        public string UIEventType { get; set; }
        public string[] Data { get; set; }

        public JournalLineUIEvent(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    public class JournalLineWorksharingEvent : JournalLine
    {
        public string SessionID { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }

        public JournalLineWorksharingEvent(int number, string raw, int block)
            : base(number, raw, block) { }
    }

    #endregion

    public class LoadedAssembly
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Vendor { get; set; }
        public string GUID { get; set; }
        public List<JournalLineAPIMessage> Events { get; set; }

        public LoadedAssembly(string name, string classname, string path, string vendor, JournalLineAPIMessage initialEvent)
        {
            Name = name;
            Class = classname;
            Path = path;
            Filename = path.Split('\\').Last();
            Vendor = vendor;
            Events = new List<JournalLineAPIMessage>() { initialEvent };
        }
    }

    public class Journal
    {
        public List<JournalLine> Lines { get; set; }
        public int LineCount;
        public string Version { get; set; }
        public string User { get; set; }
        public int BlockCount { get; set; }
        public string Path { get; set; }
        public string Build { get; set; }
        public string Branch { get; set; }
        public string Machine { get; set; }
        public string OSVersion { get; set; }

        public double? ProcessingTime { get; set; }


        public Journal(
            List<JournalLine> lines,
            string version,
            string username,
            int blockcount,
            string path,
            string build,
            string branch,
            string machinename,
            string osversion)
        {
            Lines = lines;
            LineCount = lines.Count;
            Version = version;
            User = username;
            BlockCount = blockcount;
            Path = path;
            Build = build;
            Branch = branch;
            Machine = machinename;
            OSVersion = osversion;

            ProcessingTime = null;
        }

        public bool ContainsAPIErrors()
        {
            List<JournalLine> lines = GetLinesByType(typeof(JournalLineAPIMessage));
            foreach (JournalLine line in lines)
            {
                JournalLineAPIMessage lineAPIMessage = line as JournalLineAPIMessage;
                if (lineAPIMessage.IsError)
                    return true;
            }

            return false;
        }

        public bool ContainsExceptions()
        {
            List<JournalLine> lines = GetLinesByType(typeof(JournalLineTimeStamp));

            foreach (JournalLine line in lines)
            {
                JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                if (lineTimeStamp.Description.StartsWith("ExceptionCode"))
                    return true;
            }

            return false;
        }

        public List<JournalLine> GetLinesByType(Type type)
        {
            List<JournalLine> result = new List<JournalLine>();

            foreach (JournalLine line in Lines)
            {
                if (line.GetType() == type)
                    result.Add(line);
            }

            return result;
        }

        public List<JournalLine> GetLinesByTypes(List<Type> types)
        {
            List<JournalLine> result = new List<JournalLine>();

            foreach (JournalLine line in Lines)
            {
                if (types.Contains(line.GetType()))
                    result.Add(line);
            }

            return result;
        }

        public DateTime GetDate() => GetDateTimeByBlock(1).Date;

        public DateTime GetDateTimeByBlock(int block)
        {
            if (block == 0)
                return DateTime.MinValue;

            List<JournalLine> lines = GetLinesByType(typeof(JournalLineTimeStamp));
            foreach (JournalLine line in lines)
            {
                if (line.Block == block)
                {
                    JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                    return lineTimeStamp.DateTime;
                }
            }

            return DateTime.MinValue;
        }

        public List<DateTime> GetDateTimeByBlocks(List<int> blocks)
        {
            List<DateTime> result = new List<DateTime>();

            if (blocks.Contains(0))
                return result;

            List<JournalLine> lines = GetLinesByType(typeof(JournalLineTimeStamp));
            foreach (JournalLine line in lines)
            {
                if (blocks.Contains(line.Block))
                {
                    JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                    result.Add(lineTimeStamp.DateTime);
                }
            }

            return result;
        }

        public List<JournalLine> GetFirstLines(int number)
        {
            return Lines.Take(number).ToList();
        }

        public List<JournalLine> GetLastLines(int number)
        {
            return Lines.Skip(Lines.Count - number).Take(number).ToList();
        }

        public List<JournalLine> GetLinesByBlock(int block)
        {
            return Lines.Where(x => x.Block == block).ToList();
        }

        public List<JournalLine> GetLinesByBlocks(List<int> blocks)
        {
            return Lines.Where(x => blocks.Contains(x.Block)).ToList();
        }

        public List<JournalLine> GetLinesByDateTime(DateTime fromDateTime, DateTime toDateTime)
        {
            List<JournalLine> result = new List<JournalLine>();

            if (fromDateTime == null || toDateTime == null)
                return result;

            List<JournalLine> lines = GetLinesByType(typeof(JournalLineTimeStamp));

            if (fromDateTime == null)
            {
                foreach (JournalLine line in lines)
                {
                    JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                    if (lineTimeStamp.DateTime < toDateTime)
                        result.Add(lineTimeStamp);
                }
            }
            else if (toDateTime == null)
            {
                foreach (JournalLine line in lines)
                {
                    JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                    if (lineTimeStamp.DateTime > fromDateTime)
                        result.Add(lineTimeStamp);
                }
            }
            else
            {
                foreach (JournalLine line in lines)
                {
                    JournalLineTimeStamp lineTimeStamp = line as JournalLineTimeStamp;
                    if (lineTimeStamp.DateTime > fromDateTime &&
                        lineTimeStamp.DateTime < toDateTime)
                        result.Add(lineTimeStamp);
                }
            }

            return GetLinesByBlocks(result.Select(x => x.Block).ToList());
        }

        public List<LoadedAssembly> GetLoadedAssemblies()
        {
            List<LoadedAssembly> loadedAssemblies = new List<LoadedAssembly>();
            Dictionary<string, LoadedAssembly> replacedCommands = new Dictionary<string, LoadedAssembly>();

            List<JournalLine> lines = GetLinesByType(typeof(JournalLineAPIMessage));

            foreach (JournalLine line in lines)
            {
                JournalLineAPIMessage lineAPIMessage = line as JournalLineAPIMessage;

                if (lineAPIMessage.MessageText.StartsWith("Starting"))
                {
                    string[] startdata1 = lineAPIMessage.MessageText
                        .Split(new string[] { "Application: " }, StringSplitOptions.None).Last()
                        .Split(new string[] { ", Class: " }, StringSplitOptions.None);
                    string[] startdata2 = startdata1.Last()
                        .Split(new string[] { ", Vendor : " }, StringSplitOptions.None);
                    string[] startdata3 = startdata2.Last()
                        .Split(new string[] { ", Assembly: " }, StringSplitOptions.None);

                    string appname = startdata1[0];
                    string classname = startdata2[0];
                    string path = startdata3.Last();
                    string vendor = startdata3[0];

                    loadedAssemblies.Add(new LoadedAssembly(appname, classname, path, vendor, lineAPIMessage));
                }
            }

            foreach (JournalLine line in lines)
            {
                JournalLineAPIMessage lineAPIMessage = line as JournalLineAPIMessage;

                if (lineAPIMessage.MessageText.StartsWith("Registering") ||
                    lineAPIMessage.MessageText.StartsWith("Unregistering") ||
                    lineAPIMessage.MessageText.StartsWith("API registering") ||
                    lineAPIMessage.MessageText.StartsWith("API unregistering"))
                {
                    string[] regevt = lineAPIMessage.MessageText
                        .Split(new string[] { "by application " }, StringSplitOptions.None).Last()
                        .Split(new string[] { " (" }, StringSplitOptions.None);

                    string appname = regevt[0];
                    string appguid = regevt[1].Split(')')[0];

                    List<LoadedAssembly> foundByGUID = loadedAssemblies.Where(x => x.GUID == appguid).ToList();

                    if (foundByGUID.Count > 0)
                    {
                        foundByGUID[0].Events.Add(lineAPIMessage);
                    }
                    else
                    {
                        List<LoadedAssembly> foundByName = loadedAssemblies.Where(x => (x.Name == appname && x.GUID != "")).ToList();
                        if (foundByName.Count > 0)
                        {
                            foundByName[0].GUID = appguid;
                            foundByName[0].Events.Add(lineAPIMessage);
                        }
                    }
                }
                else if (lineAPIMessage.MessageText.StartsWith("Replacing"))
                {
                    string replname = lineAPIMessage.MessageText
                        .Split(new string[] { "from application '" }, StringSplitOptions.None).Last()
                        .Split('\'')[0];
                    string replcommand = lineAPIMessage.MessageText
                        .Split(new string[] { "Replacing command id '" }, StringSplitOptions.None).Last()
                        .Split('\'')[0];

                    List<LoadedAssembly> foundByName = loadedAssemblies.Where(x => (x.Name == replname)).ToList();
                    if (foundByName.Count > 0)
                    {
                        foundByName[0].Events.Add(lineAPIMessage);
                        replacedCommands[replcommand] = foundByName[0];
                    }
                }
                else if (lineAPIMessage.MessageText.StartsWith("Restoring"))
                {
                    string restcommand = lineAPIMessage.MessageText
                        .Split(new string[] { "Restoring command id '" }, StringSplitOptions.None).Last()
                        .Split('\'')[0];

                    if (replacedCommands.ContainsKey(restcommand))
                        replacedCommands[restcommand].Events.Add(lineAPIMessage);
                }
                else if (lineAPIMessage.MessageText.StartsWith("Added pushbutton"))
                {
                    string pbpath = lineAPIMessage.MessageText
                        .Split(new string[] { ", " }, StringSplitOptions.None).Last()
                        .Replace("assembly: ", "")
                        .Replace("assembly ", "");

                    List<LoadedAssembly> foundByPath = loadedAssemblies.Where(x => (x.Path == pbpath)).ToList();
                    if (foundByPath.Count > 0)
                    {
                        foundByPath[0].Events.Add(lineAPIMessage);
                    }
                    else
                    {
                        string pbpath2 = pbpath.Split('\\').Last();
                        List<LoadedAssembly> foundByFilename = loadedAssemblies.Where(x => (x.Filename == pbpath2)).ToList();
                        if (foundByFilename.Count > 0)
                            foundByFilename[0].Events.Add(lineAPIMessage);
                    }
                }
                else if (lineAPIMessage.MessageText.StartsWith("System."))
                {
                    string excguid = lineAPIMessage.MessageText
                        .Split(new string[] { "). Changes made by this handler are going to be discarded." }, StringSplitOptions.None)[0]
                        .Split(new string[] { " (" }, StringSplitOptions.None).Last();

                    List<LoadedAssembly> foundByGUID = loadedAssemblies.Where(x => (x.GUID == excguid)).ToList();
                    if (foundByGUID.Count > 0)
                    {
                        foundByGUID[0].Events.Add(lineAPIMessage);
                    }
                }
            }

            return loadedAssemblies;
        }

        public int GetMaxRAMPeak()
        {
            return GetLinesByType(typeof(JournalLineMemoryMetrics))
                .Cast<JournalLineMemoryMetrics>()
                .Max(x => x.RAMPeak);
        }

        public int GetMaxVMPeak()
        {
            return GetLinesByType(typeof(JournalLineMemoryMetrics))
                .Cast<JournalLineMemoryMetrics>()
                .Max(x => x.VMPeak);
        }

        public int GetMinRAMAvailable()
        {
            return GetLinesByType(typeof(JournalLineMemoryMetrics))
                .Cast<JournalLineMemoryMetrics>()
                .Min(x => x.RAMAvailable);
        }

        public int GetMinVMAvailable()
        {
            return GetLinesByType(typeof(JournalLineMemoryMetrics))
                .Cast<JournalLineMemoryMetrics>()
                .Min(x => x.VMAvailable);
        }

        public DateTime GetSessionTime()
        {
            IEnumerable<JournalLineTimeStamp> ts = GetLinesByType(typeof(JournalLineTimeStamp))
                .Cast<JournalLineTimeStamp>();

            return ts.Last().DateTime - ts.First().DateTime;
        }

        public TimeSpan GetStartupTime()
        {
            DateTime first_ts = GetDateTimeByBlock(1);

            int startupBlock = GetLinesByType(typeof(JournalLineCommand))
                .Cast<JournalLineCommand>()
                .Where(x => x.CommandId == "ID_REVIT_MODEL_BROWSER_OPEN" || x.CommandId == "ID_REVIT_FILE_OPEN")
                .Select(x => x.Block)
                .First();

            return GetDateTimeByBlock(startupBlock) - first_ts;
        }

        public bool IsInPlaybackMode()
        {
            int count = GetLinesByType(typeof(JournalLineTimeStamp))
                .Cast<JournalLineTimeStamp>()
                .Where(x => x.Description.StartsWith("started journal file playback"))
                .Count();

            return count > 0;
        }

        public string StripComments(bool preserveTimeStamps)
        {
            List<Type> desiredTypes = new List<Type>
            {
                typeof(JournalLineAddinEvent),
                typeof(JournalLineCommand),
                typeof(JournalLineData),
                typeof(JournalLineDirective),
                typeof(JournalLineKeyboardEvent),
                typeof(JournalLineMiscCommand),
                typeof(JournalLineMouseEvent),
                typeof(JournalLineUIEvent),
            };

            if (preserveTimeStamps)
                desiredTypes.Add(typeof(JournalLineTimeStamp));

            string[] result = Lines
                .Where(x => desiredTypes.Contains(x.GetType()))
                .Select(x => x.RawText)
                .ToArray();

            return string.Join("\n", result);
        }

        public bool WasPlaybackInterrupted()
        {
            int count = GetLinesByType(typeof(JournalLineTimeStamp))
                .Cast<JournalLineTimeStamp>()
                .Where(x => x.Description.StartsWith("stopped at line"))
                .Where(x => x.Description.EndsWith("journal file playback"))
                .Count();

            return count > 0;
        }

        public bool WasSessionTerminatedProperly()
        {
            int count = GetLinesByType(typeof(JournalLineTimeStamp))
                .Cast<JournalLineTimeStamp>()
                .Where(x => x.Description == "finished recording journal file")
                .Count();

            return count > 0;
        }

def JournalFromPath(path):

    line = None
	try:

        processing_started = time.time()

        lineObjs = []

        jVersion = None

        jUsername = None

        jMachineName = None

        jOSVersion = None

        jPath = None

        sysinfoStarted = False

        commandCount = 0

        i = 1

        b = 0
		# Round 1: Create line objects
		with open(path, 'r') as lines:
			for line in lines:
				line = line.lstrip().rstrip('\n').rstrip('\x00')
				# ignore empty lines
				if len(line) < 2: pass
                elif line.startswith("'C ") or line.startswith("'H ") or line.startswith("'E "):

                    b += 1
					lineObjs.append(JournalTimeStamp(i, line, b))
				elif ":< API_SUCCESS { " in line: lineObjs.append(JournalAPIMessage(i, line, b, False))
				elif ":< API_ERROR { " in line: lineObjs.append(JournalAPIMessage(i, line, b, True))
				elif ":: Delta VM: " in line or line.startswith("' 0:< Initial VM: "): lineObjs.append(JournalLineMemoryMetrics(i, line, b))
				elif ":< GUI Resource Usage GDI: " in line: lineObjs.append(JournalGUIResourceUsage(i, line, b))
				elif line.startswith("' [Jrn.BasicFileInfo]"): lineObjs.append(JournalBasicFileInfo(i, line, b))
				elif line.startswith("Jrn.Data "): lineObjs.append(JournalData(i, line, b))
				elif line.startswith("Jrn.Directive "): lineObjs.append(JournalDirective(i, line, b))
				elif line.startswith("Jrn.Command "):

                    lineObjs.append(JournalCommand(i, line, b))
					# We need to count commands so we can grab JournalSystemInformation lines
					if commandCount< 2: commandCount += 1

                elif line.startswith("Jrn.Key "): lineObjs.append(JournalKeyboardEvent(i, line, b))
				elif line.startswith("Jrn.AddInEvent "): lineObjs.append(JournalAddinEvent(i, line, b))
				elif line.startswith('Jrn.Wheel') or line.startswith('Jrn.MouseMove') or line.startswith('Jrn.LButtonUp') or line.startswith('Jrn.LButtonDown') or line.startswith('Jrn.LButtonDblClk') or line.startswith('Jrn.MButtonUp') or line.startswith('Jrn.MButtonDown') or line.startswith('Jrn.MButtonDblClk') or line.startswith('Jrn.RButtonUp') or line.startswith('Jrn.RButtonDown') or line.startswith('Jrn.RButtonDblClk') or line.startswith('Jrn.Scroll'):

                    lineObjs.append(JournalMouseEvent(i, line, b))
				elif line.startswith('Jrn.Activate') or line.startswith('Jrn.AppButtonEvent') or line.startswith('Jrn.Browser') or line.startswith('Jrn.CheckBox') or line.startswith('Jrn.Close') or line.startswith('Jrn.ComboBox') or line.startswith('Jrn.DropFiles') or line.startswith('Jrn.Edit') or line.startswith('Jrn.Grid') or line.startswith('Jrn.ListBox') or line.startswith('Jrn.Maximize') or line.startswith('Jrn.Minimize') or line.startswith('Jrn.PropertiesPalette') or line.startswith('Jrn.PushButton') or line.startswith('Jrn.RadioButton') or line.startswith('Jrn.RibbonEvent') or line.startswith('Jrn.SBTrayAction') or line.startswith('Jrn.Size') or line.startswith('Jrn.SliderCtrl') or line.startswith('Jrn.TabCtrl') or line.startswith('Jrn.TreeCtrl') or line.startswith('Jrn.WidgetEvent'):

                    lineObjs.append(JournalUIEvent(i, line, b))
				elif ":< SLOG $" in line: lineObjs.append(JournalWorksharingEvent(i, line, b))
				# append linebreaks to previous line
				elif lineObjs[-1].RawText[-1] == "_": lineObjs[-1].RawText = lineObjs[-1].RawText[:-1] + line
# append linebreaks in commands
                elif line[0] == ",": lineObjs[-1].RawText = (lineObjs[-1].RawText + line).replace(" _,",",")

                elif line[0] == "'":
					# append linebreaks in API Messages
					if line[1] != " " and lineObjs[-1].Type == 'JournalAPIMessage' and not lineObjs[-1].RawText.endswith("}"): lineObjs[-1].RawText += " " + line[1:]
                    elif sysinfoStarted:
						if ":< PROCESSOR INFORMATION:" in line: 
							sysinfoType = "Processor"
							lineObjs.append(JournalComment(i, line, b))
						elif ":< VIDEO CONTROLLER INFORMATION:" in line: 
							sysinfoType = "VideoController"
							lineObjs.append(JournalComment(i, line, b))
						elif ":< PRINTER INFORMATION:" in line: 
							sysinfoType = "Printer"
							lineObjs.append(JournalComment(i, line, b))
						elif ":< PRINTER CONFIGURATION INFORMATION:" in line: 
							sysinfoType = "PrinterConfiguration"
							lineObjs.append(JournalComment(i, line, b))
						elif " INFORMATION:" in line: 
							sysinfoType = "Unknown"
							lineObjs.append(JournalComment(i, line, b))
						elif ":<    " in line: 
							if line.split(":<    ")[-1].startswith(" ") : lineObjs.append(JournalComment(i, line, b))
							else: lineObjs.append(JournalSystemInformation(i, line, b, sysinfoType))
						else: lineObjs.append(JournalComment(i, line, b))
					else:
						if not sysinfoStarted:
							if ":< OPERATING SYSTEM INFORMATION:" in line: 
								sysinfoStarted = True
                                sysinfoType = 'OperatingSystem'

                        lineObjs.append(JournalComment(i, line, b))
				else: lineObjs.append(JournalMiscCommand(i, line, b))
				i += 1				
		jBlockCount = b
# Round 2: Process raw multiline text and fill type-specific attributes
        machineNameFound = False

        OSVersionFound = False
        sysinfoItem = 0
		for line in lineObjs:
			if line.Type == 'JournalAPIMessage':
				line.MessageText = line.RawText.split("{ ")[1].split(" }")[0].strip()
				if line.MessageText.startswith("Registered an external service"): line.MessageType = "RegisteredExternalService"
				elif line.MessageText.startswith("Registered an external server") or line.MessageText.startswith("An external server has been registered"): line.MessageType = "RegisteredExternalServer"
				elif line.MessageText.startswith("Starting External DB Application"): line.MessageType = "StartingExternalDBApp"
				elif line.MessageText.startswith("Starting External Application"): line.MessageType = "StartingExternalApp"
				elif line.MessageText.startswith("Registering"): line.MessageType = "RegisteringEvent"
				elif line.MessageText.startswith("Replacing command id"): line.MessageType = "ReplacingCommandID"
				elif line.MessageText.startswith("API registering command"): line.MessageType = "RegisteringCommandEvent"
				elif line.MessageText.startswith("Added pushbutton"): line.MessageType = "AddedPushbutton"
				elif line.MessageText.startswith("Unregistering"): line.MessageType = "UnregisteringEvent"
				elif line.MessageText.startswith("Restoring command id"): line.MessageType = "RestoringCommandID"
				elif line.MessageText.startswith("API unregistering command"): line.MessageType = "UnregisteringCommandEvent"
				elif line.MessageText.startswith("System."): line.MessageType = "Exception"
				else: line.MessageType = "Unknown"
			elif line.Type == 'JournalDirective':
				d1 = line.RawText.split('"  , ')
				if len(d1) > 1:

                    KeyCandidate = d1[0][15:]
					# Allow for different formatting in Revit 2022
					if KeyCandidate.startswith('"'): KeyCandidate = KeyCandidate[1:]
                    line.Key = KeyCandidate
					for d2 in d1[1].split(","):

                        line.Values.append(d2.strip().replace('"',''))
				else:
					# Very rare case where a multiline error msg is inserted between key and values
					d1 = line.RawText.split('"')
					line.Key = d1[1]
                    line.Values.append(d1[2])
				# Add Revit version to journal metadata
				if line.Key == 'Version': jVersion = int (line.Values[0][:4])
# Add username to journal metadata
                elif line.Key == 'Username': jUsername = line.Values[0]
            elif line.Type == 'JournalData':
				d1 = line.RawText.split('"  , ')
				KeyCandidate = d1[0][10:]
				# Allow for different formatting in Revit 2022
				if KeyCandidate.startswith('"'): KeyCandidate = KeyCandidate[1:]
                line.Key = KeyCandidate
}
}