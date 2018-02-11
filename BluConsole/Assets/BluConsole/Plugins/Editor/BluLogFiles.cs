using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace BluConsole.Editor
{

	[CreateAssetMenu()]
    public class BluLogFiles : ScriptableObject
    {
    
        [SerializeField] private Texture _windowIcon;

        public Texture WindowIcon { get { return _windowIcon; } }

        public static BluLogFiles Instance
        {
            get
            {
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(BluLogFiles).ToString());
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return (BluLogFiles) AssetDatabase.LoadAssetAtPath(assetPath, typeof(BluLogFiles));
            }
        }
    
    }

}
