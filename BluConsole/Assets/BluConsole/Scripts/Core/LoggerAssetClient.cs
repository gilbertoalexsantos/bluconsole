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

[Serializable]
public class LoggerAssetClient : ScriptableObject, ILogger
{

    public static readonly int MAX_LOGS = 3000;

    [SerializeField] private List<LogInfo> _logsInfo = new List<LogInfo>();
    [SerializeField] private List<LogInfo> _dirtyLogsBeforeCompile = new List<LogInfo>();
    [SerializeField] private List<CountedLog> _countedLogs = new List<CountedLog>();
    private Dictionary<LogInfo, CountedLog> _collapsedLogs = new Dictionary<LogInfo, CountedLog>(new LogInfoComparer());
    [SerializeField] private bool _isClearOnPlay = true;
    [SerializeField] private bool _isPauseOnError = false;
    [SerializeField] private int _qtNormalLogs = 0;
    [SerializeField] private int _qtWarningLogs = 0;
    [SerializeField] private int _qtErrorLogs = 0;
    [SerializeField] private int _qtNormalCountedLogs = 0;
    [SerializeField] private int _qtWarningCountedLogs = 0;
    [SerializeField] private int _qtErrorCountedLogs = 0;
    [SerializeField] private bool _isCompiling = false;
    [SerializeField] private bool _isPlaying = false;

    public event Action OnNewLogOrTrimLogEvent;
    public event Action OnBeforeCompileEvent;
    public event Action OnAfterCompileEvent;
    public event Action OnBeginPlayEvent;

    public List<LogInfo> LogsInfo
    {
        get
        {
            return _logsInfo;
        }
    }

    public List<CountedLog> CountedLogs
    {
        get
        {
            return _countedLogs;
        }
    }

    public bool IsClearOnPlay
    {
        get
        {
            return _isClearOnPlay;
        }
        set
        {
            _isClearOnPlay = value;
        }
    }

    public bool IsPauseOnError
    {
        get
        {
            return _isPauseOnError;
        }
        set
        {
            _isPauseOnError = value;
        }
    }

    public int QtNormalLogs
    {
        get
        {
            return _qtNormalLogs;
        }
    }

    public int QtNormalCountedLogs
    {
        get
        {
            return _qtNormalCountedLogs;
        }
    }

    public int QtWarningLogs
    {
        get
        {
            return _qtWarningLogs;
        }
    }

    public int QtWarningCountedLogs
    {
        get
        {
            return _qtWarningCountedLogs;
        }
    }

    public int QtErrorLogs
    {
        get
        {
            return _qtErrorLogs;
        }
    }

    public int QtErrorCountedLogs
    {
        get
        {
            return _qtErrorCountedLogs;
        }
    }

    public int QtLogs
    {
        get
        {
            return QtNormalLogs + QtWarningLogs + QtErrorLogs;
        }
    }

    public int QtCountedLogs
    {
        get
        {
            return QtNormalCountedLogs + QtWarningCountedLogs + QtErrorCountedLogs;
        }
    }

    public static LoggerAssetClient GetOrCreate()
    {
        var loggerAsset = ScriptableObject.FindObjectOfType<LoggerAssetClient>();

        if (loggerAsset == null)
            loggerAsset = ScriptableObject.CreateInstance<LoggerAssetClient>();

        return loggerAsset;
    }

    public void Clear()
    {
        _logsInfo.Clear();
        _countedLogs.Clear();
        _collapsedLogs.Clear();
        _qtNormalLogs = _qtWarningLogs = _qtErrorLogs = 0;
        _qtNormalCountedLogs = _qtWarningCountedLogs = _qtErrorCountedLogs = 0;
    }

    public void Clear(
        Predicate<LogInfo> cmp)
    {
        _countedLogs.Clear();
        _collapsedLogs.Clear();
        _qtNormalLogs = _qtWarningLogs = _qtErrorLogs = 0;
        _qtNormalCountedLogs = _qtWarningCountedLogs = _qtErrorCountedLogs = 0;

        List<LogInfo> newLogs = new List<LogInfo>(_logsInfo.Count);
        for (int i = 0; i < _logsInfo.Count; i++)
        {
            LogInfo log = _logsInfo[i];
            if (cmp(log))
                continue;

            newLogs.Add(log);
            IncreaseLogCount(log.LogType);

            CountedLog countedLog;
            if (_collapsedLogs.TryGetValue(log, out countedLog))
            {
                countedLog.Quantity++;
            }
            else
            {
                countedLog = new CountedLog(log, 1);
                _countedLogs.Add(countedLog);
                _collapsedLogs.Add(log, countedLog);
                IncreaseCountedLogCount(log.LogType);
            }
        }

        _logsInfo = newLogs;
    }

    public void ClearExceptCompileErrors()
    {
        Clear((log) => !(log.IsCompileMessage && log.LogType == BluLogType.Error));
    }

    private void OnEnable()
    {
        EditorApplication.playmodeStateChanged -= OnPlaymodeStateChanged;
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
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
        }
    }

    private void OnBeforeCompile()
    {
        OnBeforeCompileEvent.SafeInvoke();

        _dirtyLogsBeforeCompile = new List<LogInfo>(LogsInfo.Where(log => log.IsCompileMessage));
    }

    private void OnAfterCompile()
    {
        OnAfterCompileEvent.SafeInvoke();

        var logsBlackList = new HashSet<LogInfo>(_dirtyLogsBeforeCompile, new LogInfoComparer());
        Clear(log => logsBlackList.Contains(log));
        _dirtyLogsBeforeCompile.Clear();
    }

    private void OnBeginPlay()
    {
        OnBeginPlayEvent.SafeInvoke();
    }

    private void OnPlaymodeStateChanged()
    {
        // TOOD: This server register logic shouldn't be dependent of any client... Change that
        LoggerServer.RegisterForUnityLogHandler();
    }

    private void TrimLogs()
    {
        bool trimmedLog = false;

        while (QtLogs > MAX_LOGS)
        {
            trimmedLog = true;

            LogInfo log = _logsInfo[0];

            DecreaseLogCount(log.LogType);

            // BNECK: We need to remove the first log... How to do that in an efficient way and without creating a lot
            //        of bad smells
            _logsInfo.RemoveAt(0);

            CountedLog countedLog;
            if (_collapsedLogs.TryGetValue(log, out countedLog))
            {
                countedLog.Quantity--;

                if (countedLog.Quantity == 0)
                {
                    // BNECK: If we change this to HashSet, we'll need an ordered HashSet...
                    for (int i = 0; i < _countedLogs.Count; i++)
                    {
                        if (_countedLogs[i].Log.Identifier == countedLog.Log.Identifier)
                        {
                            _countedLogs.RemoveAt(i);
                            DecreaseCountedLogCount(countedLog.Log.LogType);
                            break;
                        }
                    }
                    _collapsedLogs.Remove(log);
                }
            }
        }

        if (trimmedLog)
            OnNewLogOrTrimLogEvent.SafeInvoke();
    }

    private void IncreaseLogCount(
        BluLogType logType)
    {
        switch (logType)
        {
        case BluLogType.Normal:
            _qtNormalLogs++;
            break;
        case BluLogType.Warning:
            _qtWarningLogs++;
            break;
        case BluLogType.Error:
            _qtErrorLogs++;
            break;
        }
    }

    private void IncreaseCountedLogCount(
        BluLogType logType)
    {
        switch (logType)
        {
        case BluLogType.Normal:
            _qtNormalCountedLogs++;
            break;
        case BluLogType.Warning:
            _qtWarningCountedLogs++;
            break;
        case BluLogType.Error:
            _qtErrorCountedLogs++;
            break;
        }
    }

    private void DecreaseLogCount(
        BluLogType logType)
    {
        switch (logType)
        {
        case BluLogType.Normal:
            _qtNormalLogs--;
            break;
        case BluLogType.Warning:
            _qtWarningLogs--;
            break;
        case BluLogType.Error:
            _qtErrorLogs--;
            break;
        }
    }

    private void DecreaseCountedLogCount(
        BluLogType logType)
    {
        switch (logType)
        {
        case BluLogType.Normal:
            _qtNormalCountedLogs--;
            break;
        case BluLogType.Warning:
            _qtWarningCountedLogs--;
            break;
        case BluLogType.Error:
            _qtErrorCountedLogs--;
            break;
        }
    }

    private void BuildCollapseLogsDictionary()
    {
        _collapsedLogs.Clear();

        foreach (CountedLog countedLog in _countedLogs)
        {
            if (!_collapsedLogs.ContainsKey(countedLog.Log))
                _collapsedLogs.Add(countedLog.Log, countedLog);
        }
    }


    #region IBluLogger implementation


    public void Log(
        LogInfo logInfo)
    {
        _logsInfo.Add(logInfo);
        IncreaseLogCount(logInfo.LogType);

        // BNECK: Unfortunally Dictionary is not serializable... So we need to construct it everytime =/
        if (_collapsedLogs.Count == 0)
            BuildCollapseLogsDictionary();

        CountedLog countedLog;
        if (_collapsedLogs.TryGetValue(logInfo, out countedLog))
        {
            countedLog.Quantity++;
        }
        else
        {
            countedLog = new CountedLog(logInfo, 1);
            _countedLogs.Add(countedLog);
            _collapsedLogs.Add(logInfo, countedLog);
            IncreaseCountedLogCount(logInfo.LogType);  
        }

        OnNewLogOrTrimLogEvent.SafeInvoke();

        TrimLogs();

        if (_isPauseOnError && logInfo.LogType == BluLogType.Error)
            Debug.Break();
    }


    #endregion


}

}

#endif
