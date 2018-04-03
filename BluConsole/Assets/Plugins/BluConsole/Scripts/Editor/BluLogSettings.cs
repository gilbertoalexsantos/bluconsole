using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace BluConsole.Editor
{

    [CreateAssetMenu()]
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
                    return null;
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return (BluLogSettings) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogSettings));
            }
        }
    
    }

}
