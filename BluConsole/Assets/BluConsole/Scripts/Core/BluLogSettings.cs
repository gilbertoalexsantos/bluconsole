using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace BluConsole.Core
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
    
}

}
