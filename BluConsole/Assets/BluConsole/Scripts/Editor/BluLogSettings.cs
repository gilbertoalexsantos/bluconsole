using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BluConsole.Editor
{

    public class BluLogSettings : ScriptableObject
    {
    
        [SerializeField] private List<BluLogFilter> _filters = new List<BluLogFilter>();

        public List<BluLogFilter> Filters { get { return _filters; } }

        public static BluLogSettings Instance
        {
            get
            {
                var settings = Resources.Load<BluLogSettings>("BluConsole/BluLogSettings");
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
