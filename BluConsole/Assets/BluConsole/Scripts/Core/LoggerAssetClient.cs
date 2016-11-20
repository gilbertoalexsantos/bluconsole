#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace BluConsole
{

[Serializable]
public class LoggerAssetClient : ScriptableObject, ILogger
{

    public static readonly int MAX_LOGS = 3000;

    [SerializeField] private List<LogInfo> _logsInfo = new List<LogInfo>();
    [SerializeField] private List<CountedLog> _countedLogs = new List<CountedLog>();
    private Dictionary<LogInfo, CountedLog> _collapsedLogs = new Dictionary<LogInfo, CountedLog>(new LogInfoComparer());
    [SerializeField] private bool _isClearOnPlay = true;
    [SerializeField] private bool _isPauseOnError = false;
    [SerializeField] private int _qtNormalLogs = 0;
    [SerializeField] private int _qtWarningLogs = 0;
    [SerializeField] private int _qtErrorLogs = 0;

    public event Action OnNewLogOrTrimLogEvent;

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

    public int QtWarningsLogs
    {
        get
        {
            return _qtWarningLogs;
        }
    }

    public int QtErrorsLogs
    {
        get
        {
            return _qtErrorLogs;
        }
    }

    public int QtLogs
    {
        get
        {
            return QtNormalLogs + QtWarningsLogs + QtErrorsLogs;
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
        _qtNormalLogs = 0;
        _qtWarningLogs = 0;
        _qtErrorLogs = 0;
    }

    public void Clear(
        Predicate<LogInfo> cmp)
    {
        _countedLogs.Clear();
        _collapsedLogs.Clear();
        _qtNormalLogs = 0;
        _qtWarningLogs = 0;
        _qtErrorLogs = 0;

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
        EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
        hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnPlaymodeStateChanged()
    {
        // TOOD: This server register logic shouldn't be dependent of any client... Change that
        LoggerServer.RegisterForUnityLogHandler();
    }

    private void TrimLogs()
    {
        bool entered = false;

        while (QtLogs > MAX_LOGS)
        {
            entered = true;

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
                            break;
                        }
                    }
                    _collapsedLogs.Remove(log);
                }
            }
        }

        if (entered)
            CalOnNewLogOrTrimLogEvent();
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

    private void BuildCollapseLogsDictionary()
    {
        _collapsedLogs.Clear();

        foreach (CountedLog countedLog in _countedLogs)
        {
            if (!_collapsedLogs.ContainsKey(countedLog.Log))
                _collapsedLogs.Add(countedLog.Log, countedLog);
        }
    }


    private void CalOnNewLogOrTrimLogEvent()
    {
        if (OnNewLogOrTrimLogEvent != null)
            OnNewLogOrTrimLogEvent();
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
        }

        CalOnNewLogOrTrimLogEvent();

        TrimLogs();

        if (_isPauseOnError && logInfo.LogType == BluLogType.Error)
            Debug.Break();
    }


    #endregion


}

}

#endif
