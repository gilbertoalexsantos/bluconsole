using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace BluConsole.Editor
{

    [CreateAssetMenu(menuName="BluConsole/BluLogSettings")]
    public class BluLogSettings : ScriptableObject
    {

        private static BluLogSettings _instance;

        public static BluLogSettings Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogSettings).ToString());
                if (guids.Length == 0) 
                    return null;
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return _instance = (BluLogSettings) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogSettings));
            }
        }
    
        [SerializeField] private List<BluLogFilter> _filters = new List<BluLogFilter>();

        public List<BluLogFilter> Filters { get { return _filters; } }
    
    }

}
