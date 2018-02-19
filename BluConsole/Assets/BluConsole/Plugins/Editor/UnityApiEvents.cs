using System;
using UnityEditor;
using UnityEngine;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

    public class UnityApiEvents : ScriptableObject
    {

        private static UnityApiEvents _instance;

        private bool isCompiling;
        private bool isPlaying;
    
        public Action OnBeforeCompileEvent;
        public Action OnAfterCompileEvent;
        public Action OnBeginPlayEvent;
        public Action OnStopPlayEvent;

        public static void GenerateInstance()
        { 
            DestroyInstance();
            _instance = ScriptableObject.CreateInstance<UnityApiEvents>();
        }

        public static void DestroyInstance()
        {
            if (_instance == null)
                return;
            GameObject.DestroyImmediate(_instance);
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

            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnBeforeCompile();
        }

        private void OnAfterCompile()
        {
            OnAfterCompileEvent.SafeInvoke();

            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnAfterCompile();
        }

        private void OnBeginPlay()
        {
            OnBeginPlayEvent.SafeInvoke();

            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnBeginPlay();
        }

        private void OnStopPlay()
        {
            OnStopPlayEvent.SafeInvoke();

            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnStopPlay();
        }

    }

}
