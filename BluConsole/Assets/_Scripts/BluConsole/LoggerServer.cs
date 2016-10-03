using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


namespace BluConsole
{

public static class LoggerServer
{

    private static HashSet<ILogger> _loggers;
    private static HashSet<KeyValuePair<string, string>> _logBlackList;

    static LoggerServer()
    {
        _loggers = new HashSet<ILogger>();
        _logBlackList = new HashSet<KeyValuePair<string, string>>() {
            new KeyValuePair<string, string>("CallLogCallback", "Application"),
            new KeyValuePair<string, string>("Internal_Log", "Debug"),
            new KeyValuePair<string, string>("Internal_Log", "DebugLogHandler"),
            new KeyValuePair<string, string>("Log*", "Debug"),
            new KeyValuePair<string, string>("Log*", "DebugLogHandler"),
            new KeyValuePair<string, string>("Log*", "Logger"),
            new KeyValuePair<string, string>("LogFormat", "Debug"),
            new KeyValuePair<string, string>("LogFormat", "DebugLogHandler"),
        };

        #if UNITY_5
        Application.logMessageReceived += UnityLogHandler;
        #else
        Application.RegisterLogCallback(UnityLogHandler);
        #endif
    }

    public static void Register(ILogger logger)
    {
        _loggers.Add(logger);
    }

    public static void Unregister(ILogger logger)
    {
        if (_loggers.Contains(logger)) {
            _loggers.Remove(logger);
        }
    }

    public static ILogger GetLoggerClient<T>()
    {
        foreach (ILogger logger in _loggers)
            if (logger is T)
                return logger;
        return null;
    }

    [StackTraceIgnore]
    private static void UnityLogHandler(string message,
                                        string stackTrace,
                                        LogType logType)
    {
        string extractedMessage = ExtractMessageFromUnityMessage(message);

        List<LogStackFrame> callStack = GetCallStack();
        if (callStack.Count == 0)
            callStack = GetCallStack(stackTrace);
        if (callStack.Count == 0)
            callStack = GetCallStackFromUnityMessage(message);
        
        BluLogType bluLogType = GetLogType(logType);
        var logInfo = new LogInfo(message, extractedMessage, callStack, bluLogType, IsCompilerError(message));
        Call(logInfo);
    }

    private static string ExtractMessageFromUnityMessage(string message)
    {
        MatchCollection match = Regex.Matches(message, @".*:.*:\s*(.*)");
        if (match.Count > 0) {
            return match[0].Groups[1].Value;
        } else {
            return message;
        }
    }

    private static bool IsCompilerError(string unityMessage)
    {
        return Regex.Match(unityMessage, @".*:\s*error.*:.*").Success;
    }

    [StackTraceIgnore]
    private static List<LogStackFrame> GetCallStack()
    {
        var callStack = new List<LogStackFrame>();

        var stackTrace = new StackTrace(true);          
        StackFrame[] stackFrames = stackTrace.GetFrames(); 

        foreach (StackFrame stackFrame in stackFrames) {
            MethodBase method = stackFrame.GetMethod();

            if (IsNoise(method))
                continue;

            callStack.Add(LogStackFrame.Create(stackFrame));
        } 

        return callStack;
    }

    private static List<LogStackFrame> GetCallStack(string unityStackTrace)
    {
        var callStack = new List<LogStackFrame>();

        Regex
            .Split(unityStackTrace, System.Environment.NewLine)
            .Where(line => !string.IsNullOrEmpty(line))
            .Where(line => LogStackFrame.CanGetInformation(line))
            .ToList()
            .ForEach(line => callStack.Add(LogStackFrame.Create(line)));

        return callStack;
    }

    private static List<LogStackFrame> GetCallStackFromUnityMessage(string message)
    {
        var callStack = new List<LogStackFrame>();

        MatchCollection match = Regex.Matches(message, @"(.*)\((\d+).*");
        if (match.Count > 0) {
            string fileRelativePath = match[0].Groups[1].Value;
            int line = Convert.ToInt32(match[0].Groups[2].Value);

            string[] filePathSplitted = fileRelativePath.Split(System.IO.Path.PathSeparator);
            string className = "";
            if (filePathSplitted.Length > 0)
                className = filePathSplitted[filePathSplitted.Length - 1];
            
            callStack.Add(new LogStackFrame(className, "", fileRelativePath, line));
        }

        return callStack;
    }

    private static bool IsNoise(MethodBase method)
    {
        if (method.IsDefined(typeof(StackTraceIgnore), true))
            return true;

        foreach (var pair in _logBlackList) {
            if (Regex.Match(method.Name, pair.Key).Success && 
                Regex.Match(method.DeclaringType.Name, pair.Value).Success)
                return true;
        }

        return false;
    }

    private static BluLogType GetLogType(LogType logType)
    {
        BluLogType bluLogType = BluLogType.Normal;

        switch (logType) {
        case LogType.Warning:
            bluLogType = BluLogType.Warning;
            break;
        case LogType.Error:
            bluLogType = BluLogType.Error;
            break;
        }

        return bluLogType;
    }

    private static void Call(LogInfo log)
    {
        foreach (ILogger logger in _loggers)
            logger.Log(log);
    }
        
}

}
