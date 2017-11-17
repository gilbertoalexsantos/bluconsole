using System;
using UnityEditor;
using UnityEngine;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

    public class UnityApiEvents : ScriptableObject
    {
        
        private bool isCompiling;
        private bool isPlaying;
    
        public Action OnBeforeCompileEvent;
        public Action OnAfterCompileEvent;
        public Action OnBeginPlayEvent;
        public Action OnStopPlayEvent;

        public static UnityApiEvents Instance
        {
            get
            {
                var loggerAsset = ScriptableObject.FindObjectOfType<UnityApiEvents>();
                if (loggerAsset == null)
                    loggerAsset = ScriptableObject.CreateInstance<UnityApiEvents>();
                return loggerAsset;
            }
        }

        private void OnEnable()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            hideFlags = HideFlags.HideAndDontSave;
        }

        private void Update()
        {
            if (EditorApplication.isCompiling && !isCompiling)
            {
                isCompiling = true;
                OnBeforeCompile();
            }
            else if (!EditorApplication.isCompiling && isCompiling)
            {
                isCompiling = false;
                OnAfterCompile();
            }

            if (EditorApplication.isPlaying && !isPlaying)
            {
                isPlaying = true;
                OnBeginPlay();
            }
            else if (!EditorApplication.isPlaying && isPlaying)
            {
                isPlaying = false;
                OnStopPlay();
            }
        }

        private void OnBeforeCompile()
        {
            OnBeforeCompileEvent.SafeInvoke();
        }

        private void OnAfterCompile()
        {
            OnAfterCompileEvent.SafeInvoke();
        }

        private void OnBeginPlay()
        {
            OnBeginPlayEvent.SafeInvoke();
        }

        private void OnStopPlay()
        {
            OnStopPlayEvent.SafeInvoke();
        }

    }

}
