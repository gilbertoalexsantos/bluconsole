#if UNITY_EDITOR

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace BluConsole
{

[Serializable]
public class LoggerAssetClient : ScriptableObject, ILogger
{

    public static readonly int MAX_LOGS = 5000;

    [SerializeField] private List<LogInfo> _logsInfo = new List<LogInfo>();
    [SerializeField] private List<CountedLog> _countedLogs = new List<CountedLog>();
    [SerializeField] private Dictionary<LogInfo, CountedLog> _collapsedLogs = 
        new Dictionary<LogInfo, CountedLog>(new LogInfoComparer());
    [SerializeField] private bool _isClearOnPlay = true;
    [SerializeField] private bool _isPauseOnError = false;
    [SerializeField] private int _qtNormalLogs = 0;
    [SerializeField] private int _qtWarningLogs = 0;
    [SerializeField] private int _qtErrorLogs = 0;

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

    public List<LogInfo> GetLogsInfoFiltered(
        string patternHelmLike)
    {
        string[] patterns = patternHelmLike.ToLower().Split(' ');
        return _logsInfo.Where(log => patterns.All(pattern => log.Message.ToLower().Contains(pattern))).ToList();
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
        LoggerServer.RegisterForUnityLogHandler();
    }

    private void TrimLogs()
    {
        while (_logsInfo.Count > MAX_LOGS)
        {
            LogInfo log = _logsInfo[0];

            DecreaseLogCount(log.LogType);
            _logsInfo.RemoveAt(0);

            CountedLog countedLog;
            if (_collapsedLogs.TryGetValue(log, out countedLog))
            {
                countedLog.Quantity--;

                if (countedLog.Quantity == 0)
                {
                    _countedLogs.RemoveAt(0);
                    _collapsedLogs.Remove(log);
                }
            }
        }
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


    #region IBluLogger implementation


    public void Log(
        LogInfo logInfo)
    {
        _logsInfo.Add(logInfo);
        IncreaseLogCount(logInfo.LogType);

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
            
        TrimLogs();
    }


    #endregion


}

}

#endif