using System;
using UnityEditor;
using UnityEngine;
using BluConsole.Extensions;


namespace BluConsole.Editor
{

public class UnityApiEvents : ScriptableObject
{

    bool isCompiling;
    bool isPlaying;
    
    public event Action OnBeforeCompileEvent;
    public event Action OnAfterCompileEvent;
    public event Action OnBeginPlayEvent;
    public event Action OnStopPlayEvent;

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

    void OnEnable()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        hideFlags = HideFlags.HideAndDontSave;
    }

    void Update()
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

    void OnBeforeCompile()
    {
        OnBeforeCompileEvent.SafeInvoke();
    }

    void OnAfterCompile()
    {
        OnAfterCompileEvent.SafeInvoke();
    }

    void OnBeginPlay()
    {
        OnBeginPlayEvent.SafeInvoke();
    }

    void OnStopPlay()
    {
        OnStopPlayEvent.SafeInvoke();
    }

}

}
