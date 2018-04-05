using System.Reflection;


namespace BluConsole.Core.UnityLoggerApi
{

    public static class BluInternalEditorUtility
    {

        public static void OpenEditorConsole()
        {
            var method = GetMethod("OpenEditorConsole");
            method.Invoke(null, null);
        }

        public static void OpenPlayerConsole()
        {
            var method = GetMethod("OpenPlayerConsole");
            method.Invoke(null, null);
        }

        private static MethodInfo GetMethod(string key)
        {
            return ReflectionCache.GetMethod(key, UnityClassType.InternalEditorUtility);
        }

    }

}
