using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BluConsole.Editor
{

public class BluLogSettings : ScriptableObject
{
    
    [SerializeField] List<BluLogFilter> filters = new List<BluLogFilter>();

    public List<BluLogFilter> Filters
    {
        get
        {
            return filters;
        }
    }

    public static BluLogSettings Instance
    {
        get
        {
            var path = "BluConsole/BluLogSettings";
            var settings = Resources.Load<BluLogSettings>(path);
            if (settings == null)
            {
                settings = CreateInstance<BluLogSettings>();
                var paths = AssetDatabase.GetAllAssetPaths();
                var settingsPath = "";
                foreach (var p in paths)
                {
                    if (p.EndsWith("BluConsole/Resources/BluConsole"))
                    {
                        settingsPath = p;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(settingsPath))
                {
                    AssetDatabase.CreateAsset(settings, settingsPath + "/BluLogSettings.asset");
                    AssetDatabase.SaveAssets ();
                    AssetDatabase.Refresh();
                }
            }

            return settings ?? CreateInstance<BluLogSettings>();
        }
    }
    
}

}
