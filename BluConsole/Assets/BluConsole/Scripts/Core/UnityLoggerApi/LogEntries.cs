using System;
using System.Reflection;


namespace BluConsole.Core.UnityLoggerApi
{

public static class LogEntries
{

    public static void Clear()
    {
        var method = GetMethod("Clear");
        method.Invoke(null, null);
    }

    public static BluLog GetCompleteLog(
        int row)
    {
        var emptyLog = LogEntry.GetNewEmptyObject;
        GetEntryInternal(row, emptyLog);
        return LogEntry.GetBluLog(emptyLog);
    }

    public static BluLog GetSimpleLog(
        int row)
    {
        int mode = 0;
        string message = "";
        GetFirstTwoLinesEntryTextAndModeInternal(row, ref mode, ref message);
        BluLog log = new BluLog();
        log.SetMessage(message);
        log.SetMode(mode);
        return log;
    }

    public static int GetEntryCount(
        int row)
    {
        var method = GetMethod("GetEntryCount");
        return (int)method.Invoke(null, new object[] { row });
    }

    public static int GetCount()
    {
        var method = GetMethod("GetCount");
        return (int)method.Invoke(null, null);
    }

    public static void GetCountsByType(
        ref int error,
        ref int warning,
        ref int normal)
    {
        var method = GetMethod("GetCountsByType");

        var parameters = new object[3] { 0, 0, 0 };
        method.Invoke(null, parameters);

        error = (int)parameters[0];
        warning = (int)parameters[1];
        normal = (int)parameters[2];
    }

    public static int StartGettingEntries()
    {
        var method = GetMethod("StartGettingEntries");
        return (int)method.Invoke(null, null);
    }

    public static void EndGettingEntries()
    {
        var method = GetMethod("EndGettingEntries");
        method.Invoke(null, null);
    }

    private static bool GetEntryInternal(
        int row,
        object output)
    {
        var method = GetMethod("GetEntryInternal");
        return (bool)method.Invoke(null, new object[] { row, output });
    }

    private static void GetFirstTwoLinesEntryTextAndModeInternal(
        int row,
        ref int mode,
        ref string text)
    {
        var method = GetMethod("GetFirstTwoLinesEntryTextAndModeInternal");

        var parameters = new object[] { row, 0, "" };
        method.Invoke(null, parameters);

        mode = (int)parameters[1];
        text = (string)parameters[2];
    }

    private static MethodInfo GetMethod(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntriesType.GetMethod(key, DefaultFlags));
        return CachedReflection.Get<MethodInfo>(key);
    }

    private static PropertyInfo GetProperty(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntriesType.GetProperty(key, DefaultFlags));
        return CachedReflection.Get<PropertyInfo>(key);
    }

    private static Type LogEntriesType
    {
        get
        {
            if (!CachedReflection.Has("LogEntries"))
                CachedReflection.Cache("LogEntries", Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll"));
            return CachedReflection.Get<Type>("LogEntries");
        }
    }

    private static BindingFlags DefaultFlags
    {
        get
        {
            return BindingFlags.Static | BindingFlags.Public;
        }
    }

}

}
