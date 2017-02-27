using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using BluConsole.Core;
using System;


[CreateAssetMenu(fileName = "BluLogSettings", menuName = "BluConsole/Settings", order = 1)]
public class BluLogSettings : ScriptableObject
{
    
    [SerializeField]
    private List<string> _filter = new List<string>();

    private List<string> _filterLower = new List<string>();

    public List<string> FilterLower { get { return _filterLower; } }

    public void CacheFilterLower()
    {
        _filterLower.Clear();
        foreach (var f in _filter)
        {
            var fLower = f.Trim().ToLower();
            if (!string.IsNullOrEmpty(fLower))
                _filterLower.Add(fLower);
        }
    }

    public static BluLogSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<BluLogSettings>("Assets/BluConsole/Assets/BluLogSettings.asset");
        return settings ?? CreateInstance<BluLogSettings>();
    }
	
}
