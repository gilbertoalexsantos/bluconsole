using UnityEngine;
using System;
using System.Collections.Generic;


namespace BluConsole.Editor
{

    [Serializable]
    public class BluLogFilter : ISerializationCallbackReceiver
    {
    
        #pragma warning disable 414, CS0649
        [SerializeField] private string _name;
        [SerializeField] private string _pattern;
        #pragma warning restore CS0649  

        private List<string> _patterns;

        public string Name { get { return string.IsNullOrEmpty(_name) ? "?" : _name; } }
        public string Pattern  {  get  { return string.IsNullOrEmpty(_pattern) ? "" : _pattern; }  }
        public List<string> Patterns { get { return _patterns = _patterns ?? new List<string>(); } }

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
