using System;
using System.Reflection;


namespace BluConsole.Core.UnityLoggerApi
{

    /*
     * Fields:
     * string condition
     * int errorNum
     * string file
     * int line
     * int mode
     * int instanceID
     * int identifier
     * int isWorldPlaying
     */
    public static class LogEntry
    {

        private static object _cachedLogEntry;

        public static object CachedLogEntry 
        { 
            get 
            {
                if (_cachedLogEntry == null)
                    _cachedLogEntry = Activator.CreateInstance(ReflectionCache.GetType(UnityClassType.LogEntry));
                return _cachedLogEntry;
            }
        }

        public static BluLog GetBluLog(object obj)
        {
            var log = new BluLog();

            var condition = (string)GetField("condition").GetValue(obj);
            log.SetMessage(condition);
            log.SetStackTrace(condition);

            var file = (string)GetField("file").GetValue(obj);
            log.SetFile(file);

            var line = (int)GetField("line").GetValue(obj);
            log.SetLine(line);

            var mode = (int)GetField("mode").GetValue(obj);
            log.SetMode(mode);

            var instanceID = (int)GetField("instanceID").GetValue(obj);
            log.SetInstanceID(instanceID);

            return log;
        }

        private static FieldInfo GetField(string key)
        {
            return ReflectionCache.GetField(key, UnityClassType.LogEntry);
        }

    }

}
