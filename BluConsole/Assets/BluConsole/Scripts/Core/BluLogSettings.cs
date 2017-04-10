using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace BluConsole.Core
{


[Serializable]
public class BluLogFilter : ISerializationCallbackReceiver
{
    
    [SerializeField] private string _name;
    [SerializeField] private string _pattern;
    private List<string> _patterns;

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(_name))
                return "?";
            return _name;
        }
    }

    public string Pattern 
    { 
        get 
        {
            if (_pattern == null)
                return "";
            return _pattern; 
        } 
    }

    public List<string> Patterns
    {
        get
        {
            return _patterns;
        }
    }

    public void OnAfterDeserialize()
    {
        _patterns = new List<string>();
        var patterns = Pattern.ToLower().Split(' ');
        foreach (var pattern in patterns)
        {
            if (string.IsNullOrEmpty(pattern))
                continue;
            _patterns.Add(pattern);
        }
    }

    public void OnBeforeSerialize()
    {
    }

}

public class BluLogSettings : ScriptableObject
{
    
    [SerializeField] private List<BluLogFilter> _filters = new List<BluLogFilter>();

    public List<BluLogFilter> Filters { get { return _filters; } }
    
}

}
