using System.Collections.Generic;


namespace BluConsole.Core.UnityLoggerApi
{

public static class CachedReflection 
{

    private static Dictionary<string, object> _cache = new Dictionary<string, object>();

    public static T Get<T>(
        string key)
    {
        return (T)_cache[key];
    }

    public static void Cache(
        string key, 
        object value)
    {
        _cache[key] = value;
    }

    public static bool Has(
        string key)
    {
        return _cache.ContainsKey(key);
    }

}

}
