using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BluConsole.Core.UnityLoggerApi
{

public enum ConsoleWindowFlag
{
    Collapse = 1,
    ClearOnPlay = 2,
    ErrorPause = 4,
    Verbose = 8,
    StopForAssert = 16,
    StopForError = 32,
    Autoscroll = 64,
    LogLevelLog = 128,
    LogLevelWarning = 256,
    LogLevelError = 512
}

public enum ConsoleWindowMode
{
    Error = 1,
    Assert = 2,
    Log = 4,
    Fatal = 16,
    DontPreprocessCondition = 32,
    AssetImportError = 64,
    AssetImportWarning = 128,
    ScriptingError = 256,
    ScriptingWarning = 512,
    ScriptingLog = 1024,
    ScriptCompileError = 2048,
    ScriptCompileWarning = 4096,
    StickyError = 8192,
    MayIgnoreLineNumber = 16384,
    ReportBug = 32768,
    DisplayPreviousErrorInStatusBar = 65536,
    ScriptingException = 131072,
    DontExtractStacktrace = 262144,
    ShouldClearOnPlay = 524288,
    GraphCompileError = 1048576,
    ScriptingAssertion = 2097152
}


public static class ConsoleWindow
{

    public static bool IsDebugError(
        int mode)
    {
        var options = new ConsoleWindowMode[] {
            ConsoleWindowMode.Error,
            ConsoleWindowMode.Assert,
            ConsoleWindowMode.Fatal,
            ConsoleWindowMode.AssetImportError,
            ConsoleWindowMode.AssetImportWarning,
            ConsoleWindowMode.ScriptingError,
            ConsoleWindowMode.ScriptCompileError,
            ConsoleWindowMode.ScriptCompileWarning,
            ConsoleWindowMode.StickyError,
            ConsoleWindowMode.ScriptingException,
            ConsoleWindowMode.GraphCompileError,
            ConsoleWindowMode.ScriptingAssertion
        };
        int mask = 0;
        for (int i = 0; i < options.Length; i++)
            mask |= (int)options[i];
        return (mode & mask) != 0;
    }

    public static bool HasFlag(
        ConsoleWindowFlag flag)
    {
        var method = GetMethod("HasFlag");
        return (bool)method.Invoke(null, new object[] {
                (int)flag
            });
    }

    public static void SetFlag(
        ConsoleWindowFlag flag,
        bool active)
    {
        var method = GetMethod("SetFlag");
        method.Invoke(null, new object[] {
                (int)flag,
                active
            });
    }

    public static bool HasMode(
        int mode,
        ConsoleWindowMode modeToCheck)
    {
        var method = GetMethod("HasMode");
        return (bool)method.Invoke(null, new object[] {
            mode,
            (int)modeToCheck
        });
    }

    private static MethodInfo GetMethod(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, ConsoleWindowType.GetMethod(key, DefaultFlags));
        return CachedReflection.Get<MethodInfo>(key);
    }

    private static PropertyInfo GetProperty(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, ConsoleWindowType.GetProperty(key, DefaultFlags));
        return CachedReflection.Get<PropertyInfo>(key);
    }

    private static Type ConsoleWindowType
    {
        get
        {
            if (!CachedReflection.Has("ConsoleWindow"))
                CachedReflection.Cache("ConsoleWindow", Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll"));
            return CachedReflection.Get<Type>("ConsoleWindow");   
        }
    }

    private static BindingFlags DefaultFlags
    {
        get
        {
            return BindingFlags.Static | BindingFlags.NonPublic;
        }
    }
	
}

}
