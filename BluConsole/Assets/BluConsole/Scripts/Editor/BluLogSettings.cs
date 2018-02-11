using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


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
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogSettings).ToString());
                if (guids.Length == 0) 
                    return GenerateBluLogSettingsAsset();
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return (BluLogSettings) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogSettings));
            }
        }

        private static BluLogSettings GenerateBluLogSettingsAsset()
        {
            var settings = CreateInstance<BluLogSettings>();
            var paths = AssetDatabase.GetAllAssetPaths();
            var settingsPath = "";
            foreach (var p in paths)
            {
                if (p.EndsWith("BluConsole"))
                {
                    settingsPath = p;
                    break;
                }
            }
            if (!string.IsNullOrEmpty(settingsPath))
            {
                AssetDatabase.CreateAsset(settings, settingsPath + "/BluLogSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            return settings;
        }
    
    }

}
