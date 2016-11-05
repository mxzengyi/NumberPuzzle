/********************************************************************
    Created:    2014/10/10
    File Base:  Log.cs
    Author:     Adam

    Purpose:    Game log class, print log to console or files.
*********************************************************************/
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;


public enum ELogLevel
{
    Verbose,
    Debug,
    Infomative,
    Key,
    Warning,
    Error,
    Fatal,
}

[Flags]
public enum ELogTag
{
    Test,
    LogSys,
    Event,
    Main,
    GameRoot,
    ResourceSys,
    PreLoadSys,
    Finance,
    SoundPlay,
    UnityLog,
    Crash,
    Task,
    Login,
    NetSys,
    CmdRsv,
    SceneLoad,
    MailSys,
    ShopSys,
    Res,
    Share,
    SelectFriends,
    Avatar,
    MsgBox,
    NetMsg,
    MsgHandler,
    AnimLoad,
    Cache,
    BattleScene,
    CharacterBase,
    AI,
    SkillEffect,
    BattleProtocol,
    SkillEffect2,
    StageData,
    HeroStarUp,
    HeroGradeUp,
    PlayerInfo,
    EquipOp,
    Buff,
    Config,
    Alchemy,
    Train,
    LeaderBoard,
    ManagerBase,
    BasePanel,
    Team,
    InputRecorder,
    System,
    Broadcast,
    UI,
    Campaign,
    Chest,
    ModelBase,
    ModelManager,
    Gift,
    TitleBar,
    Realm,
    Apollo,
    Home,
    Tavern,
    NewbieGuide,
    Champion,
    Item,
    Ams,
    Announcement,
    VersionUpdate,
    UIJump,
    XgNotice,
    Setting,
    ActivityTimeModel,
    MainLogic,
    WhiteList,
    SceneSwitch,
    DiamondLottery,
    NewerQuest,
    TimeDaemon,
    LoadingTime,

    Max,
}

public class LogMessage
{
    public ELogLevel level;
    public ELogTag tag;

    public DateTime time;

    public string content;

    public StackFrame[] stack;
    public string stackStr;

    public string packedContent;

    public void Reset()
    {
        content = "";
        stack = null;
        stackStr = null;
        packedContent = null;
    }
}

public class LogConstant
{
    public const ELogLevel PRINT_STACK_LEVEL = ELogLevel.Warning;
    public const ELogLevel UPLOAD_EXCEPTION_LEVEL = ELogLevel.Error;
}

public class Log 
{
    private const string cLogPrefix = "[VLOG]";
    private const int DEFAULT_CHAR_BUFFER_LEN = 2048;
    private const int SUPPRESS_LOG_MAX_LINE = 8;
    private const int SUPPRESS_LOG_SAMPLE_CHAR_COUNT = 32;

    private byte[] _logBitmap = new byte[(int)ELogTag.Max];

    //开启日志输出到文件
    private bool _enableLogFile = true;

    //初始化日志总控级别，只有高于此级别的日志才会有后续逻辑
    private ELogLevel _logLevel = ELogLevel.Verbose;

    //调试面板日志级别控制
    private ELogLevel _logLevelPanel = ELogLevel.Verbose;
    
    //控制台日志级别控制
    private ELogLevel _logLevelConsole = ELogLevel.Verbose;

    private string _sameLogStr;
    private int _sameLogCnt;

    private StringBuilder _logStringBuilder;

    private LogWriter _logWriter;

    private static Log sLog = null;
    private static object _lockHelper = new object();

    private Log()
    {
        //Application.RegisterLogCallback(HandleLog);

        _logStringBuilder = new StringBuilder(DEFAULT_CHAR_BUFFER_LEN);

        if (_enableLogFile)
        {
            _logWriter = new LogWriter();
        }

        // Enable all tag default.

        // MMUtil.Memset在IOS上面会导致JIT错误，这里暂时以循环代替
        for (int i = 0; i < _logBitmap.Length; ++i)
        {
            _logBitmap[i] = 1;
        }

        Application.RegisterLogCallback(HandleLog);

      //  MMUtil.Memset(_logBitmap, 1, _logBitmap.Length);
    }

    public static Log GetInstance()
    {
        if (sLog == null)
        {
            lock (_lockHelper)
            {
                if (sLog == null)
                {
                    sLog = new Log();
                }
            }
        }

        return sLog;
    }

    public static void Destroy()
    {
        if (sLog != null)
        {
            sLog.CloseLogWriter();
            sLog = null;
        }
    }

    public LogWriter GetLogWriter()
    {
        return _logWriter;
    }

    public void SetTagEnable(ELogTag tag, bool enabled)
    {
        _logBitmap[(int)tag] = (byte)(enabled ? 0x01 : 0x00);
    }

    public void SetLogLevel(ELogLevel level)
    {
        _logLevel = level;
    }

    public void SetLogLevelPanel(ELogLevel level)
    {
        _logLevelPanel = level;
    }

    public void SetLogLevelConsole(ELogLevel level)
    {
        _logLevelConsole = level;
    }

    public ELogLevel GetLogLevelConsole()
    {
        return _logLevelConsole;
    }

    private void CloseLogWriter()
    {
        if (_logWriter != null)
        {
            _logWriter.Close();
            _logWriter = null;
        }
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString == null || logString.StartsWith(cLogPrefix))
        {
            return;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        // PC开发测试环境忽略错误 http get http data exception
        if (logString.Contains("You are trying to load data from a www stream which had the following error when downloading"))
        {
            return;
        }
#endif

        ELogLevel level = ELogLevel.Verbose;
        switch (type)
        {
            case LogType.Log:
                level = ELogLevel.Verbose;
                break;
            case LogType.Warning:
                level = ELogLevel.Warning;
                break;
            case LogType.Error:
                level = ELogLevel.Error;
                break;
            case LogType.Exception:
                level = ELogLevel.Error;
                break;
            case LogType.Assert:
                level = ELogLevel.Warning;
                break;
        }

        if (stackTrace != null)
        {
            Print(level, ELogTag.UnityLog, logString + "\n" + stackTrace);
        }
        else
        {
            Print(level, ELogTag.UnityLog, logString);
        }
    }

    public static void P(string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Verbose, ELogTag.UnityLog, format, args);
        }
    }

    public static void V(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Verbose, tag, format, args);
        }
    }

    public static void D(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Debug, tag, format, args);
        }
    }

    public static void I(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Infomative, tag, format, args);
        }
    }

    public static void W(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Warning, tag, format, args);
        }
    }

    public static void E(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Error, tag, format, args);
        }
    }

    public static void F(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Fatal, tag, format, args);
        }
    }

    public static void K(ELogTag tag, string format, params object[] args)
    {
        if (sLog != null)
        {
            sLog.Print(ELogLevel.Key, tag, format, args);
        }
    }

    public void Print(ELogLevel level, ELogTag tag, string format, params object[] args)
    {
        if (level < _logLevel)
        {
            return;
        }

        if (_logBitmap[(int)tag] == 0)
        {
            return;
        }

        string content = format;
        try
        {
            content = string.Format(format, args);
        }
        catch (Exception e)
        {
            content = e.ToString();
            level = ELogLevel.Error;
        }

        {
            string sameStr = content.Substring(0, Mathf.Min(SUPPRESS_LOG_SAMPLE_CHAR_COUNT, content.Length));
            if (string.IsNullOrEmpty(_sameLogStr) || !sameStr.Equals(_sameLogStr))
            {
                _sameLogStr = sameStr;
                _sameLogCnt = 0;
            }
            else
            {
                _sameLogCnt++;
            }

            if (_sameLogCnt == SUPPRESS_LOG_MAX_LINE)
            {
                content = "<ignore duplicate log message>";
            }
            else if (_sameLogCnt > SUPPRESS_LOG_MAX_LINE)
            {
                return;
            }
        }

        LogMessage msg = null;

        if (_logWriter != null)
        {
            msg = _logWriter.GetMessage();
        }
        
        msg.time = DateTime.Now;
        msg.level = level;
        msg.tag = tag;
        msg.content = content;

        lock (_logStringBuilder)
        {
            if (level >= LogConstant.PRINT_STACK_LEVEL)
            {
                // All print call contains 2 frame which in this file.
                StackTrace stackTrace = new StackTrace(2, true);
                StackFrame[] stackFrames = stackTrace.GetFrames();
                msg.stack = stackFrames;

                GetStackStr(_logStringBuilder, msg.stack);
                msg.stackStr = _logStringBuilder.ToString();
            }

            PackMessage(msg, _logStringBuilder);
        }

        // Output Log to file
        if (_logWriter != null)
        {
            _logWriter.AddMessage(msg);
        }

        if (level >= _logLevelConsole)
        {
            // Output Log console.
            PrintMessage(msg);
        }
    }

    private StringBuilder GetStackStr(StringBuilder builder, StackFrame[] stackFrames)
    {
        StringBuilder sb = builder;
        if (sb == null)
        {
            sb = new StringBuilder();
        }

        if (stackFrames == null)
        {
            sb.Append("NullStack");
            return sb;
        }

        sb.Length = 0;

        for (int i = 0; i < stackFrames.Length; ++i)
        {
            StackFrame stackFrame = stackFrames[i];
            MethodBase method = stackFrame.GetMethod();

            if (method == null || method.DeclaringType == null)
            {
                continue;
            }

            string typeName = method.DeclaringType.FullName;
            if (typeName.Equals(this.GetType().FullName))
            {
                continue;
            }

            string methodName = method.Name;
            sb.AppendFormat("{0}::{1}(", typeName, methodName);

            ParameterInfo[] parameterInfos = method.GetParameters();
            int parameterCount = parameterInfos.Length;
            for (int j = 0; j < parameterCount; j++)
            {
                ParameterInfo parameterInfo = parameterInfos[j];
                if (j > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(parameterInfo.ParameterType.FullName);
            }

            string fileName = stackFrame.GetFileName();
            int fileLineNumber = stackFrame.GetFileLineNumber();
            sb.AppendFormat(")({0}:{1})\n", fileName, fileLineNumber);
        }

        return sb;
    }

    private StringBuilder TransInLine(string prefix, string content, string stack, StringBuilder builder)
    {
        StringBuilder sb = builder;
        sb.Length = 0;

        string[] contentArray = content.Split('\n');
        for (int i = 0; i < contentArray.Length; ++i)
        {
            sb.Append(prefix);
            sb.Append(contentArray[i]);
            sb.Append('\n');
        }

        if (string.IsNullOrEmpty(stack))
        {
            return sb;
        }

        //sb.Append('\n');

        string[] stackArray = stack.Split('\n');
        for (int i = 0; i < stackArray.Length; ++i)
        {
            sb.Append(prefix);
            sb.Append("    ");
            sb.Append(stackArray[i]);

            sb.Append('\n');
        }

        return sb;
    }

#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern void SaveUnityStackTrace(string info);

    [DllImport("__Internal")]
    public static extern void CrashApplication();
#endif


    private void PrintMessage(LogMessage msg)
    {
        string logStr = msg.packedContent;
        string tagStr = msg.tag.ToString();

        switch (msg.level)
        {
            case ELogLevel.Fatal:
            case ELogLevel.Error:
                {
                    #if UNITY_EDITOR || UNITY_STANDALONE
                    UnityEngine.Debug.LogError(logStr);
#endif
                    if (logStr != null)
                    {
                        //Debug.LogError(logStr, tagStr);

                        //if (MainConfig.IsDevelopmentMode)
                        {
                            // 只有在开发模式下才提示黑底红字
                            //if (StayWarningDialog.Instance != null)
                            //{
                            //    StayWarningDialog.Instance.ShowStayWarning(logStr);
                            //}
                        }
#if RELEASE_MODE
#if UNITY_IPHONE
                        SaveUnityStackTrace(logStr);
                        CrashApplication();
#endif
#else

#endif
                    }

                }
                break;
            case ELogLevel.Warning:
                {
                    #if UNITY_EDITOR || UNITY_STANDALONE
                    UnityEngine.Debug.LogWarning(logStr);
#endif

                    if (msg.level >= _logLevelPanel &&
                        logStr != null /*&& MainConfig.IsDevelopmentMode*/)
                    {
                        //Debug.LogWarning(logStr, tagStr);
                    }
                }
                break;
            case ELogLevel.Key:
            case ELogLevel.Infomative:
            case ELogLevel.Debug:
            case ELogLevel.Verbose:
                {
                    #if UNITY_EDITOR || UNITY_STANDALONE
                    UnityEngine.Debug.Log(logStr);
#endif
                    if (msg.level >= _logLevelPanel &&
                        logStr != null /*&& MainConfig.IsDevelopmentMode*/)
                    {
                        //Debug.Log(logStr, tagStr);
                    }
                }
                break;
            default:
                {
                    #if UNITY_EDITOR || UNITY_STANDALONE
                    if (!string.IsNullOrEmpty(logStr))
                    {
                        UnityEngine.Debug.LogWarning(
                        String.Format("unknown log(level:{0}): {1}", msg.level, logStr));
                    }
#endif
                }
                break;
        }
    }

    private void PackMessage(LogMessage msg, StringBuilder builder)
    {
        System.DateTime time = msg.time;

        string curTime = string.Format("{0:00}-{1:00} {2:00}:{3:00}:{4:00}.{5:000}",
            time.Month, time.Day, time.Hour, time.Minute, time.Second, time.Millisecond);

        ELogLevel level = msg.level;
        string prefix = string.Format("{0} {1} {2}/{3}: ", cLogPrefix, curTime, level.ToString()[0], msg.tag);

        string stack = null;
        if (msg.level >= LogConstant.PRINT_STACK_LEVEL && msg.level != ELogLevel.Key)
        {
            stack = msg.stackStr;
        }

        StringBuilder stringBuilder = TransInLine(prefix, msg.content, stack, builder);
        msg.packedContent = stringBuilder.ToString();
    }

    static public void UploadUserAction(string userActionType, ELogTag eLogTag, string content, int elapsedTime = 0, int consumedBytes = 0)
    {
        sLog.GetLogWriter().UploadUserAction(userActionType, eLogTag, content, elapsedTime, consumedBytes);
    }


}


public class LogWriter
{
    private const string LOG_FILE_SUFFIX = "log";
    private const int MAX_LOG_FILE_COUNT = 20;
    private const int MAX_LOG_FILE_SIZE = 2 * 1024 * 1024;
    private const int FLUSH_FILE_MESSAGE_COUNT = 5;
    private const int MAX_CACHED_MESSAGE_COUNT = 128;
    private const int MAX_UPLOAD_BYTE_COUNT = 10 * 1024;

    // 10s
    private const int MAX_WRITE_TIME = 10 * 1000;

    private Encoding ENCODING = Encoding.UTF8;

    private string _pathRoot;
    private string _fileName;

    private byte[] _buffer;

    private bool _writerStop;
    private Thread _writerThread;
    
    private object _waitLock;

    private FileStream _streamWriter;

    private Queue _messageWorkQueue;
    private ArrayList _recycleList;

    public LogWriter()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        _pathRoot = "Log";
#else
        _pathRoot = Path.Combine(Application.persistentDataPath, "Log");
#endif

        _messageWorkQueue = Queue.Synchronized(new Queue());
        _recycleList = ArrayList.Synchronized(new ArrayList());

        _waitLock = new object();
        _writerStop = false;

        _writerThread = new Thread(new ThreadStart(WriterLoop));
        _writerThread.Priority = System.Threading.ThreadPriority.Lowest;
        _writerThread.Start();
    }

    public void AddMessage(LogMessage message)
    {
        if (message == null)
        {
            return;
        }

        _messageWorkQueue.Enqueue(message);

        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }
    }

    public LogMessage GetMessage()
    {
        LogMessage message = null;

        lock (_recycleList.SyncRoot)
        {
            if (_recycleList.Count > 0)
            {
                message = _recycleList[0] as LogMessage;
            }
        }

        if (message == null)
        {
            message = new LogMessage();
        }

        message.Reset();

        return message;
    }

    private string GenerateFileName()
    {
        string fileName = null;
        do
        {
            DateTime now = DateTime.Now;

            string name = string.Format("{0:00}-{1:00}{2:00}-{3:00}{4:00}{5:00}.{6}",
                    now.Year % 100, now.Month, now.Day, now.Hour, now.Minute, now.Second, LOG_FILE_SUFFIX);
            string path = Path.Combine(_pathRoot, name);

            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                fileName = path;
                Thread.Sleep(2000);
                break;
            }
        } while (true);

        return fileName;
    }

    private void CleanExpiredFile()
    {
        string[] files = null;

        try
        {
            files = Directory.GetFiles(_pathRoot, "*." + LOG_FILE_SUFFIX);
        }
        catch (Exception e)
        {
            //Debug.Log(e.ToString());
            return;
        }

        if (files == null || files.Length < MAX_LOG_FILE_COUNT - 1)
        {
            return;
        }

        Array.Sort<string>(files);

        int fileCount = files.Length;
        for (int i = 0; i < fileCount - MAX_LOG_FILE_COUNT; i++)
        {
            // Remove the file
            string fileToDelete = files[i];

            //Debug.Log("delete old log file " + fileToDelete);
            File.Delete(files[i]);
        }
    }

    private bool CreateStream()
    {
        _fileName = GenerateFileName();
        //Debug.Log("generate file name " + _fileName);

        try
        {
            if (!Directory.Exists(_pathRoot))
            {
                Directory.CreateDirectory(_pathRoot);
            }

            _streamWriter = new FileStream(_fileName, FileMode.OpenOrCreate);
            //Debug.Log("create file stream " + _fileName);
        }
        catch (Exception e)
        {
            //Debug.Log(string.Format("create log file({0}) failed!\n {1}", _fileName, e.ToString()));
            _streamWriter = null;
            return false;
        }

        return true;
    }

    private void WriterLoop()
    {
        int flushCounter = 0;

        //Debug.Log("log writer thread started");
        while (true)
        {
            // We make sure all log flush into file.
            if (_writerStop && _messageWorkQueue.Count == 0)
            {
                break;
            }

            bool timeout = false;

            if (!_writerStop && _messageWorkQueue.Count == 0)
            {
                lock (_waitLock)
                {
                    // Add wait time to avoid the last message can't be write to file.
                    timeout = Monitor.Wait(_waitLock, MAX_WRITE_TIME);
                }
            }

            if (timeout)
            {
                Flush();
            }

            LogMessage message = null;
            if (_messageWorkQueue.Count > 0)
            {
                message = _messageWorkQueue.Dequeue() as LogMessage;
            }

            if (message == null)
            {
                continue;
            }

            WriteLine(message.packedContent);

            flushCounter++;

            if (flushCounter >= FLUSH_FILE_MESSAGE_COUNT)
            {
                Flush();
            }

//             if (message.level >= LogConstant.UPLOAD_EXCEPTION_LEVEL && _messageHandler != null)
//             {
//                 UploadMessage uploadMessage = new UploadMessage();
//                 uploadMessage.RecordMessage = message;
//                 uploadMessage.ContextBefore = PrepareBuffer(message);
// 
//                 // Fill stack.
//                 if (message.stack != null)
//                 {
//                     uploadMessage.StackInfo = EncodeStack(message.stack);
//                 }
// 
//                 Message uiMessage = new Message();
//                 uiMessage.Callback = UploadException;
//                 uiMessage.Param = uploadMessage;
//                 uiMessage.What = (int)MessageId.MsgUploadException;
// 
//                 _messageHandler.PostMessage(uiMessage);
//             }

            _recycleList.Add(message);
        }

        CloseWriter();

        //Debug.Log("log writer thread finished");
    }

    private void UploadException(int what, object param)
    {
        UploadMessage message = param as UploadMessage;
        if (message == null)
        {
            return;
        }

        LogMessage logMessage = message.RecordMessage;

//        DengTaUploadException.UploadException(logMessage.tag.ToString(), logMessage.packedContent);

//         Dictionary<string, string> userInfo = new Dictionary<string, string>();
//         userInfo.Add(logMessage.tag.ToString(), logMessage.content);
// 
//         bool result = UserAction.UploadUserAction("ScriptError", false, -1, -1, userInfo, true);
//         if (!result)
//         {
//             Log.D(ELogTag.LogSys, "upload user action failed!!");
//         }

//         string messageContent = string.Format("{0}:{1}", logMessage.tag, logMessage.content);
//         DengTaUploadException.UploadExceptionWithStack(messageContent, message.ContextBefore, message.StackInfo);
    }

    /// 上报玩家行为数据
    public void UploadUserAction(string actionType, ELogTag eLogTag, string content, int elapsedTime = 0, int consumedBytes = 0)
    {
//         UploadMessage uploadMessage = new UploadMessage();
//         uploadMessage.ContextBefore = actionType;
// 
//         LogMessage logMessage = new LogMessage();
//         logMessage.tag = eLogTag;
//         logMessage.content = content;
//         uploadMessage.RecordMessage = logMessage;
// 
//         uploadMessage.ElaspedTime = elapsedTime;
//         uploadMessage.ConsumedBytes = consumedBytes;
// 
//         Message uiMessage = new Message();
//         uiMessage.Callback = UploadUserActionCallBack;
//         uiMessage.Param = uploadMessage;
//         uiMessage.What = (int)MessageId.MsgUpdateUserAction;
// 
//         _messageHandler.PostMessage(uiMessage);        
    }


    private void UploadUserActionCallBack(int what, object param)
    {
        UploadMessage uploadMessage = param as UploadMessage;
        if (null == uploadMessage)
        {
            return;
        }

        string userActionType = uploadMessage.ContextBefore;

        LogMessage logMessage = uploadMessage.RecordMessage;
        if (null == logMessage
            || null == logMessage.tag
            || null == logMessage.content)
        {
            return;
        }

        Dictionary<string, string> userInfo = new Dictionary<string, string>();
        userInfo.Add(logMessage.tag.ToString(), logMessage.content);

        //MSDK.Instance.reportEvent(userActionType, userInfo, false);
    }

    private string EncodeStack(StackFrame[] stackFrames, StringBuilder builder = null)
    {
        StringBuilder sb = builder;
        if (sb == null)
        {
            sb = new StringBuilder();
        }

        if (stackFrames == null)
        {
            return sb.ToString();
        }

        sb.Length = 0;

        for (int i = 0; i < stackFrames.Length; ++i)
        {
            StackFrame stackFrame = stackFrames[i];
            MethodBase method = stackFrame.GetMethod();

            if (method == null || method.DeclaringType == null)
            {
                continue;
            }

            string className = method.DeclaringType.FullName;

            string methodName = method.Name;
            sb.AppendFormat("{0}/{1}(", className, methodName);

            ParameterInfo[] parameterInfos = method.GetParameters();
            int parameterCount = parameterInfos.Length;
            for (int j = 0; j < parameterCount; j++)
            {
                ParameterInfo parameterInfo = parameterInfos[j];
                if (j > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(parameterInfo.ParameterType.FullName);
            }
            sb.Append(")");

            string fileName = stackFrame.GetFileName();
            int fileLineNumber = stackFrame.GetFileLineNumber();
            sb.AppendFormat("/{0}/{1};", fileName, fileLineNumber);
        }

        return sb.ToString();
    }

    private string PrepareBuffer(LogMessage logMessage)
    {
        int bufferByteCount = 0;
        StringBuilder builder = new StringBuilder();

        Stack<LogMessage> messageStack = new Stack<LogMessage>();

        int messageByteCount = Encoding.Unicode.GetByteCount(logMessage.packedContent);
        if (messageByteCount >= MAX_UPLOAD_BYTE_COUNT)
        {
            return builder.ToString();
        }

        messageStack.Push(logMessage);
        builder.Insert(0, logMessage.packedContent);
        bufferByteCount += messageByteCount;

        lock (_recycleList.SyncRoot)
        {
            int recycleCount = _recycleList.Count;
            for (int i = recycleCount - 1; i >= 0; i--)
            {
                LogMessage message = _recycleList[i] as LogMessage;
                messageByteCount = Encoding.Unicode.GetByteCount(message.packedContent);
                if (bufferByteCount + messageByteCount >= MAX_UPLOAD_BYTE_COUNT)
                {
                    break;
                }

                builder.Insert(0, message.packedContent);
                bufferByteCount += messageByteCount;
            }
        }

        return builder.ToString();
    }

    private void WriteLine(string line)
    {
        bool result = VerifyFile();
        if (!result)
        {
            UnityEngine.Debug.LogWarning("Can't verify file");
            return;
        }

        if (string.IsNullOrEmpty(line))
        {
            line = "\n";
        }

        int maxByteCount = ENCODING.GetByteCount(line);
        if (_buffer == null || _buffer.Length < maxByteCount)
        {
            _buffer = new byte[maxByteCount];
        }

        // ArgumentException???
        int actByteCount = ENCODING.GetBytes(line, 0, line.Length, _buffer, 0);
        //Debug.Log(string.Format("write {0} bytes log to file ", actByteCount));

        try
        {
            _streamWriter.Write(_buffer, 0, actByteCount);

            if (!line.EndsWith("\n"))
            {
                _streamWriter.WriteByte((byte)'\n');
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }

    private void Flush()
    {
        try
        {
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }

    public void Close()
    {
        _writerStop = true;

        lock (_waitLock)
        {
            Monitor.PulseAll(_waitLock);
        }

        _writerThread.Join();
    }

    private bool VerifyFile()
    {
        CloseStreamIf();

        if (_streamWriter == null)
        {
            CleanExpiredFile();
            CreateStream();
        }

        if (_streamWriter == null)
        {
            return false;
        }

        return true;
    }

    private void CloseStreamIf()
    {
        if (_streamWriter == null)
        {
            return;
        }

        if (_streamWriter.Length >= MAX_LOG_FILE_SIZE)
        {
            CloseWriter();
        }
    }

    private void CloseWriter()
    {
        if (_streamWriter == null)
        {
            return;
        }

        try
        {
            _streamWriter.Flush();
            _streamWriter.Close();
            _streamWriter = null;
        }
        catch (Exception e)
        {
            //Debug.LogException(e);
        }
    }

    private class UploadMessage
    {
        public LogMessage RecordMessage;
        public string ContextBefore;

        public string StackInfo;

        // 事件处理耗时，单位毫秒
        public int ElaspedTime;

        // 事件流量，单位字节
        public int ConsumedBytes;
    }

}

/// <summary>
/// Util to clear array.
/// </summary>
static class MMUtil
{
    static MMUtil()
    {
        var dynamicMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
            null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(Util), true);

        var generator = dynamicMethod.GetILGenerator();
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Ldarg_1);
        generator.Emit(OpCodes.Ldarg_2);
        generator.Emit(OpCodes.Initblk);
        generator.Emit(OpCodes.Ret);

        MemsetDelegate = (Action<IntPtr, byte, int>)dynamicMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>));
    }

    public static void Memset(byte[] array, byte what, int length)
    {
        var gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
        MemsetDelegate(gcHandle.AddrOfPinnedObject(), what, length);
        gcHandle.Free();
    }

    public static void ForMemset(byte[] array, byte what, int length)
    {
        for (var i = 0; i < length; i++)
        {
            array[i] = what;
        }
    }

    private static Action<IntPtr, byte, int> MemsetDelegate;
}
