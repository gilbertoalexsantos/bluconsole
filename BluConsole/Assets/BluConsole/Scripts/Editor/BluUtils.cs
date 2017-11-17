using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;


namespace BluConsole.Editor
{

    public static class BluUtils
    {

        private static readonly List<string> defaultIgnorePrefixes = new List<string>()
        {
            "UnityEngine.Debug"
        };

        public static void OpenFileOnEditor(string path, int line)
        {
            var asset = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
            if (asset == null)
                return;

            AssetDatabase.OpenAsset(asset.GetInstanceID(), line);
            Resources.UnloadAsset(asset);
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
