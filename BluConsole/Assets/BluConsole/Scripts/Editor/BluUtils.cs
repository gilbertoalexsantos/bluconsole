using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace BluConsole.Editor
{

public static class BluUtils
{

    static List<string> defaultIgnorePrefixes = new List<string>()
    {
        "UnityEngine.Debug"
    };

    public static GUIContent GUIContent(this string text)
    {
        return new GUIContent(text);
    }

    public static void OpenFileOnEditor(string path, int line)
    {
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
        if (System.IO.File.Exists(filename))
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(path, line);
    }

    public static List<string> StackTraceIgnorePrefixs
    {
        get
        {
            var ret = new List<string>(defaultIgnorePrefixes);
            var stackTraceIgnoreType = typeof(StackTraceIgnore);

            var assembly = Assembly.GetAssembly(stackTraceIgnoreType);
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | 
                                                    BindingFlags.NonPublic |
                                                    BindingFlags.Static |
                                                    BindingFlags.Instance |
                                                    BindingFlags.Default))
                {
                    if (method.GetCustomAttributes(stackTraceIgnoreType, true).Length > 0)
                    {
                        var key = string.Format("{0}:{1}", method.DeclaringType.FullName, method.Name);
                        ret.Add(key);
                    }
                }
            }

            return ret;
        }
    }
	
}

}
