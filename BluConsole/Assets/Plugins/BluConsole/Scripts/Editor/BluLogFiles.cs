using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace BluConsole.Editor
{

	[CreateAssetMenu(menuName="BluConsole/BluLogFiles")]
    public class BluLogFiles : ScriptableObject
    {

        private static BluLogFiles _instance;

        public static BluLogFiles Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogFiles).ToString());
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return _instance = (BluLogFiles) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogFiles));
            }
        }
    
        [SerializeField] private Texture _windowIcon;

        public Texture WindowIcon { get { return _windowIcon; } }
    
    }

}
