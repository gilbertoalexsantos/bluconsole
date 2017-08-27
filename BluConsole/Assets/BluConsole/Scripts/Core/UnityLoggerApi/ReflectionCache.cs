using System;
using System.Reflection;
using System.Collections.Generic;


namespace BluConsole.Core.UnityLoggerApi
{

    public enum UnityClassType
    {
        ConsoleWindow = 0,
        LogEntries    = 1,
        LogEntry      = 2,
    }

    public static class ReflectionCache
    {

        static Dictionary<UnityClassType, string> cacheStrUnityClassType = new Dictionary<UnityClassType, string>()
        {
            { UnityClassType.ConsoleWindow, "0" },
            { UnityClassType.LogEntries,    "1" },
            { UnityClassType.LogEntry,      "2" },
        };

        static Dictionary<Type, string> cacheStrType = new Dictionary<Type, string>()
        {
            { typeof(MethodInfo),   "0" },
            { typeof(PropertyInfo), "1" },
            { typeof(FieldInfo),    "2" },
        };

        static Dictionary<string, object> cache = new Dictionary<string, object>();

        public static MethodInfo GetMethod(string key, UnityClassType type)
        {
            var cacheKey = GetKey<MethodInfo>(key, type);
            if (!Has(cacheKey))
                cache[cacheKey] = GetType(type).GetMethod(key, GetFlags(type));
            return (MethodInfo)cache[cacheKey];
        }

        public static PropertyInfo GetProperty(string key, UnityClassType type)
        {
            var cacheKey = GetKey<PropertyInfo>(key, type);
            if (!Has(cacheKey))
                cache[cacheKey] = GetType(type).GetProperty(key, GetFlags(type));
            return (PropertyInfo)cache[cacheKey];
        }

        public static FieldInfo GetField(string key, UnityClassType type)
        {
            var cacheKey = GetKey<FieldInfo>(key, type);
            if (!Has(cacheKey))
                cache[cacheKey] = GetType(type).GetField(key);
            return (FieldInfo)cache[cacheKey];
        }

        public static Type GetType(UnityClassType type)
        {
            var cacheKey = type.ToString();
            if (!Has(cacheKey))
                cache[cacheKey] = GetTypeFromAssembly(type);
            return (Type)cache[cacheKey];
        }

        static string GetKey<T>(string key, UnityClassType type)
        {
            return String.Format("{0}:{1}:{2}", cacheStrUnityClassType[type], cacheStrType[typeof(T)], key);
        }

        static bool Has(string key)
        {
            return cache.ContainsKey(key);
        }

        static Type GetTypeFromAssembly(UnityClassType type)
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
                default:
                    return default(Type);
            }
#endif
        }

        static BindingFlags GetFlags(UnityClassType type)
        {
            switch (type)
            {
                case UnityClassType.ConsoleWindow:
                    return BindingFlags.Static | BindingFlags.NonPublic;
                case UnityClassType.LogEntries:
                    return BindingFlags.Static | BindingFlags.Public;
                case UnityClassType.LogEntry:
                    return BindingFlags.Default;
                default:
                    return BindingFlags.Default;
            }
        }

    }

}
