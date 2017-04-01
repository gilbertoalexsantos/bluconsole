namespace BluConsole.Core.UnityLoggerApi
{

public static class UnityLoggerServer
{

    public static void Clear()
    {
        LogEntries.Clear();
    }

    public static BluLog GetCompleteLog(
        int row)
    {
        return LogEntries.GetCompleteLog(row);
    }

    public static BluLog GetSimpleLog(
        int row)
    {
        return LogEntries.GetSimpleLog(row);
    }

    public static int GetLogCount(
        int row)
    {
        return LogEntries.GetEntryCount(row);
    }

    public static int StartGettingLogs()
    {
        return LogEntries.StartGettingEntries();
    }

    public static void StopGettingsLogs()
    {
        LogEntries.EndGettingEntries();
    }

    public static int GetCount()
    {
        return LogEntries.GetCount();
    }

    public static void GetCount(
        ref int normal,
        ref int warning,
        ref int error)
    {
        LogEntries.GetCountsByType(ref error, ref warning, ref normal);
    }

    public static bool IsDebugError(
        int mode)
    {
        return ConsoleWindow.IsDebugError(mode);
    }

    public static bool HasFlag(
        ConsoleWindowFlag flag)
    {
        return ConsoleWindow.HasFlag(flag);
    }

    public static void SetFlag(
        ConsoleWindowFlag flag,
        bool active)
    {
        ConsoleWindow.SetFlag(flag, active);
    }

    public static bool HasMode(
        int mode,
        ConsoleWindowMode modeToCheck)
    {
        return ConsoleWindow.HasMode(mode, modeToCheck);
    }

}

}
