using System;
using System.Reflection;
using System.Collections.Generic;


namespace BluConsole.Core.UnityLoggerApi
{

    public enum UnityClassType
    {
        ConsoleWindow         = 0,
        LogEntries            = 1,
        LogEntry              = 2,
        InternalEditorUtility = 3,
    }

    public static class ReflectionCache
    {

        private static Dictionary<UnityClassType, string> _cacheStrUnityClassType = new Dictionary<UnityClassType, string>()
        {
            { UnityClassType.ConsoleWindow,         "0" },
            { UnityClassType.LogEntries,            "1" },
            { UnityClassType.LogEntry,              "2" },
            { UnityClassType.InternalEditorUtility, "3" },
        };

        private static Dictionary<Type, string> _cacheStrType = new Dictionary<Type, string>()
        {
            { typeof(MethodInfo),   "0" },
            { typeof(PropertyInfo), "1" },
            { typeof(FieldInfo),    "2" },
        };

        private static Dictionary<string, object> _cache = new Dictionary<string, object>();

        public static MethodInfo GetMethod(string key, UnityClassType type)
        {
            var cacheKey = GetKey<MethodInfo>(key, type);
            if (!Has(cacheKey))
                _cache[cacheKey] = GetType(type).GetMethod(key, Flags);
            return (MethodInfo)_cache[cacheKey];
        }

        public static PropertyInfo GetProperty(string key, UnityClassType type)
        {
            var cacheKey = GetKey<PropertyInfo>(key, type);
            if (!Has(cacheKey))
                _cache[cacheKey] = GetType(type).GetProperty(key, Flags);
            return (PropertyInfo)_cache[cacheKey];
        }

        public static FieldInfo GetField(string key, UnityClassType type)
        {
            var cacheKey = GetKey<FieldInfo>(key, type);
            if (!Has(cacheKey))
                _cache[cacheKey] = GetType(type).GetField(key);
            return (FieldInfo)_cache[cacheKey];
        }

        public static Type GetType(UnityClassType type)
        {
            var cacheKey = type.ToString();
            if (!Has(cacheKey))
                _cache[cacheKey] = GetTypeFromAssembly(type);
            return (Type)_cache[cacheKey];
        }

        private static string GetKey<T>(string key, UnityClassType type)
        {
            return string.Format("{0}:{1}:{2}", _cacheStrUnityClassType[type], _cacheStrType[typeof(T)], key);
        }

        private static bool Has(string key)
        {
            return _cache.ContainsKey(key);
        }

        private static Type GetTypeFromAssembly(UnityClassType type)
        {
#if UNITY_2017_1_OR_NEWER
            switch (type)
            {
                case UnityClassType.ConsoleWindow:
                    return Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
                case UnityClassType.LogEntries:
                    return Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
                case UnityClassType.LogEntry:
                    return Type.GetType("UnityEditor.LogEntry,UnityEditor.dll");
                case UnityClassType.InternalEditorUtility:
                    return Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor.dll");
                default:
                    return default(Type);
            }
#else
            switch (type)
            {
                case UnityClassType.ConsoleWindow:
                    return Type.GetType("UnityEditor.ConsoleWindow,UnityEditor.dll");
                case UnityClassType.LogEntries:
                    return Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
                case UnityClassType.LogEntry:
                    return Type.GetType("UnityEditorInternal.LogEntry,UnityEditor.dll");
                case UnityClassType.InternalEditorUtility:
                    return Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor.dll");
                default:
                    return default(Type);
            }
#endif
        }

        private static BindingFlags Flags
        {
            get
            {
                return BindingFlags.Public |
                       BindingFlags.NonPublic |
                       BindingFlags.Static |
                       BindingFlags.Instance |
                       BindingFlags.Default;
            }
        }

    }

}
