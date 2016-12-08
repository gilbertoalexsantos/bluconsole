/*
  MIT License

  Copyright (c) [2016] [Gilberto Alexandre dos Santos]

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/


#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


namespace BluConsole.Core
{


public static class LoggerServer
{

    private static HashSet<ILogger> _loggers = new HashSet<ILogger>();

    // The KEY is REGEX like
    private static HashSet<KeyValuePair<string, string>> _logBlackList =
        new HashSet<KeyValuePair<string, string>>() {
            new KeyValuePair<string, string>("CallLogCallback", "UnityEngine.Application"),
            new KeyValuePair<string, string>(".*", "UnityEngine.Debug*"),
            new KeyValuePair<string, string>("Log*", "UnityEngine.Logger"),
        };

    static LoggerServer()
    {
        RegisterForUnityLogHandler();
    }

    // TODO: Change that to be automatically... Ideally, the server shouldn't need to know anyone... But, the static
    // constructor ain't being called after starting the game and stopping.
    public static void RegisterForUnityLogHandler()
    {
        Application.logMessageReceived -= LoggerServer.UnityLogHandler;
        Application.logMessageReceived += LoggerServer.UnityLogHandler;
    }

    public static void Register(
        ILogger logger)
    {
        _loggers.Add(logger);
    }

    public static void Unregister(
        ILogger logger)
    {
        if (_loggers.Contains(logger))
            _loggers.Remove(logger);
    }

    public static ILogger GetLoggerClient<T>()
    {
        foreach (ILogger logger in _loggers)
            if (logger is T)
                return logger;
        return null;
    }

    [StackTraceIgnore]
    public static void UnityLogHandler(
        string message,
        string stackTrace,
        LogType logType)
    {
        bool isCompileError = string.IsNullOrEmpty(stackTrace);

        List<LogStackFrame> callStack = GetCallStack();
        if (callStack.Count == 0)
            callStack = GetCallStack(stackTrace);
        if (callStack.Count == 0)
            callStack = GetCallStackFromUnityMessage(message);

        BluLogType bluLogType = GetLogType(logType);
        var logInfo = new LogInfo(message, callStack, bluLogType, isCompileError);
        Call(logInfo);
    }

    [StackTraceIgnore]
    private static List<LogStackFrame> GetCallStack()
    {
        var callStack = new List<LogStackFrame>();

        var stackTrace = new StackTrace(true);
        StackFrame[] stackFrames = stackTrace.GetFrames();

        foreach (StackFrame stackFrame in stackFrames)
        {
            MethodBase method = stackFrame.GetMethod();

            if (IsNoise(method))
                continue;

            callStack.Add(LogStackFrame.Create(stackFrame));
        }

        return callStack;
    }

    private static List<LogStackFrame> GetCallStack(
        string unityStackTrace)
    {
        var callStack = new List<LogStackFrame>();

        // I love that piece of code :D
        Regex
            .Split(unityStackTrace, System.Environment.NewLine)
            .Where(line => !string.IsNullOrEmpty(line))
            .Where(line => LogStackFrame.CanGetInformation(line))
            .ToList()
            .ForEach(line => callStack.Add(LogStackFrame.Create(line)));

        return callStack;
    }

    private static List<LogStackFrame> GetCallStackFromUnityMessage(
        string message)
    {
        var callStack = new List<LogStackFrame>();

        MatchCollection match = Regex.Matches(message, @"(.*)\((\d+).*");
        if (match.Count > 0)
        {
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

    private static bool IsNoise(
        MethodBase method)
    {
        if (method.IsDefined(typeof(StackTraceIgnore), true))
            return true;

        foreach (var pair in _logBlackList)
        {
            if (Regex.Match(method.Name, pair.Key).Success &&
                Regex.Match(method.DeclaringType.ToString(), pair.Value).Success)
                return true;
        }

        return false;
    }

    private static BluLogType GetLogType(
        LogType logType)
    {

        BluLogType bluLogType = BluLogType.Normal;

        switch (logType)
        {
        case LogType.Warning:
            bluLogType = BluLogType.Warning;
            break;
        case LogType.Error:
            bluLogType = BluLogType.Error;
            break;
        case LogType.Exception:
            bluLogType = BluLogType.Error;
            break;
        }

        return bluLogType;
    }

    private static void Call(
        LogInfo log)
    {
        foreach (ILogger logger in _loggers)
            logger.Log(log);
    }

}

}

#endif