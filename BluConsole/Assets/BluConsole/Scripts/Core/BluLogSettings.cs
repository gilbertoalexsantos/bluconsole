using UnityEngine;
using System.Collections.Generic;


namespace BluConsole.Core
{

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
    
}

}
