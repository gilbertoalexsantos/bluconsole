using System;
using UnityEditor;
using UnityEngine;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

public class UnityApiEvents : ScriptableObject
{

    private bool _isCompiling;
    private bool _isPlaying;
    
    public event Action OnBeforeCompileEvent;
    public event Action OnAfterCompileEvent;
    public event Action OnBeginPlayEvent;
    public event Action OnStopPlayEvent;

    public static UnityApiEvents GetOrCreate()
    {
        var loggerAsset = ScriptableObject.FindObjectOfType<UnityApiEvents>();

        if (loggerAsset == null)
            loggerAsset = ScriptableObject.CreateInstance<UnityApiEvents>();

        return loggerAsset;
    }

    private void OnEnable()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        hideFlags = HideFlags.HideAndDontSave;
    }

    private void Update()
    {
        if (EditorApplication.isCompiling && !_isCompiling)
        {
            _isCompiling = true;
            OnBeforeCompile();
        }
        else if (!EditorApplication.isCompiling && _isCompiling)
        {
            _isCompiling = false;
            OnAfterCompile();
        }

        if (EditorApplication.isPlaying && !_isPlaying)
        {
            _isPlaying = true;
            OnBeginPlay();
        }
        else if (!EditorApplication.isPlaying && _isPlaying)
        {
            _isPlaying = false;
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
