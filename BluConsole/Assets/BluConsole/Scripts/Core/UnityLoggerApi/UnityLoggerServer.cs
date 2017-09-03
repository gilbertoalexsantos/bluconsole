namespace BluConsole.Core.UnityLoggerApi
{

    public static class UnityLoggerServer
    {

        public static void Clear()
        {
            LogEntries.Clear();
        }

        public static BluLog GetCompleteLog(int row)
        {
            var log = LogEntries.GetCompleteLog(row);
            log.LogType = GetLogType(log);
            return log;
        }

        public static BluLog GetSimpleLog(int row)
        {
            var log = LogEntries.GetSimpleLog(row);
            log.LogType = GetLogType(log);
            return log;
        }

        public static int GetLogCount(int row)
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

        public static void GetCount(ref int normal, ref int warning, ref int error)
        {
            LogEntries.GetCountsByType(ref error, ref warning, ref normal);
        }

        public static bool IsDebugError(int mode)
        {
            return ConsoleWindow.IsDebugError(mode);
        }

        public static bool HasFlag(ConsoleWindowFlag flag)
        {
            return ConsoleWindow.HasFlag(flag);
        }

        public static void SetFlag(ConsoleWindowFlag flag, bool active)
        {
            ConsoleWindow.SetFlag(flag, active);
        }

        public static bool HasMode(int mode, ConsoleWindowMode modeToCheck)
        {
            return ConsoleWindow.HasMode(mode, modeToCheck);
        }

        static BluLogType GetLogType(BluLog log)
        {
            int mode = log.Mode;
            if (UnityLoggerServer.HasMode(mode, (ConsoleWindowMode)UnityLoggerServer.GetLogMask(BluLogType.Error)))
                return BluLogType.Error;
            else if (UnityLoggerServer.HasMode(mode, (ConsoleWindowMode)UnityLoggerServer.GetLogMask(BluLogType.Warning)))
                return BluLogType.Warning;
            else
                return BluLogType.Normal;
        }

        static int GetLogMask(BluLogType type)
        {
            switch (type)
            {
            case BluLogType.Normal:
                return 1028;
            case BluLogType.Warning:
                return 4736;
            default:
                return 3148115;
            }
        }

    }

}
