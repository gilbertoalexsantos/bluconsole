using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace BluConsole.Editor
{

[Serializable]
public class BluLogFilter : ISerializationCallbackReceiver
{
    
    [SerializeField] string name = "";
    [SerializeField] string pattern = "";
    List<string> patterns;

    public string Name
    {
        get
        {
            return string.IsNullOrEmpty(name) ? "?" : name;
        }
    }

    public string Pattern 
    { 
        get 
        {
            return pattern == null ? "" : pattern;
        } 
    }

    public List<string> Patterns
    {
        get
        {
            return patterns = patterns == null ? new List<string>() : patterns;
        }
    }

    public void OnAfterDeserialize()
    {
        Patterns.Clear();

        foreach (var pattern in Pattern.ToLower().Split(' '))
        {
            if (string.IsNullOrEmpty(pattern))
                continue;
            Patterns.Add(pattern);
        }
    }

    public void OnBeforeSerialize()
    {
    }

}

}
