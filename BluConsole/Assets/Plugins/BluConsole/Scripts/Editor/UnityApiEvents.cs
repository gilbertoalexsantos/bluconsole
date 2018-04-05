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
            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnBeforeCompile();
        }

        private void OnAfterCompile()
        {
            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnAfterCompile();
        }

        private void OnBeginPlay()
        {
            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnBeginPlay();
        }

        private void OnStopPlay()
        {
            if (BluConsoleEditorWindow.Instance != null)
                BluConsoleEditorWindow.Instance.OnStopPlay();
        }

    }

}
