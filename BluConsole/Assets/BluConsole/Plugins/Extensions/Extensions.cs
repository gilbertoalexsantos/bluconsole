using System;
using UnityEngine;


namespace BluConsole.Extensions
{

    public static class Extensions 
    {

        public static void SafeInvoke(this Action action)
        {
            if (action != null)
                action();
        }
        
        public static void AddCallback(this Action a, Action cb)
        {
            a -= cb;
            a += cb;
        }
        
        public static GUIContent GUIContent(this string text)
        {
            return new GUIContent(text);
        }
    
    }

}
