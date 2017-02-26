/*
  MIT License

  Copyright (c) [2016] [Gilberto Alexandre dos Santos]

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all
  copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  SOFTWARE.
*/


#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BluConsole.Core
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
            OnStopPlayEvent();
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

}

}

#endif
