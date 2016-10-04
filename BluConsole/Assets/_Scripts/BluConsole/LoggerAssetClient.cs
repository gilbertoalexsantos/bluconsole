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

    [SerializeField] private List<LogInfo> _logsInfo = new List<LogInfo>();
    [SerializeField] private bool _isClearOnPlay = true;
    [SerializeField] private bool _isPauseOnError = false;
    [SerializeField] private int _qtNormalLogs = 0;
    [SerializeField] private int _qtWarningLogs = 0;
    [SerializeField] private int _qtErrorLogs = 0;
    [SerializeField] private bool _wasPlaying = false;

    public List<LogInfo> LogsInfo
    {
        get
        {
            return _logsInfo;
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

    public List<LogInfo> GetLogsInfoFiltered(string patternHelmLike)
    {
        string[] patterns = patternHelmLike.ToLower().Split(' ');
        return _logsInfo.Where(log => patterns.All(pattern => log.Message.ToLower().Contains(pattern))).ToList();
    }

    public void Clear()
    {
        _logsInfo.Clear();
        _qtNormalLogs = 0;
        _qtWarningLogs = 0;
        _qtErrorLogs = 0;
    }

    public void Clear(Predicate<LogInfo> cmp)
    {
        var logsToRemove = _logsInfo.Where(log => cmp(log)).ToList();
        foreach (var log in logsToRemove) {
            switch (log.LogType) {
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

        _logsInfo.RemoveAll(cmp);
    }

    public void ClearExceptErrors()
    {
        _logsInfo = _logsInfo.Where(log => log.IsCompilerError).ToList();
        _qtNormalLogs = 0;
        _qtWarningLogs = 0;
        _qtErrorLogs = _logsInfo.Count;
    }

    public void ClearCompileErrors()
    {
        int oldQtMessages = _logsInfo.Count;
        _logsInfo = _logsInfo.Where(log => !log.IsCompilerError).ToList();
        _qtErrorLogs -= oldQtMessages - _logsInfo.Count;
    }

    private void OnEnable()
    {
        EditorApplication.playmodeStateChanged += OnProcessStateChange;
        hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnProcessStateChange()
    {
        if (!_wasPlaying && EditorApplication.isPlayingOrWillChangePlaymode && _isClearOnPlay)
            Clear();
        _wasPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
    }


    #region IBluLogger implementation

    public void Log(LogInfo logInfo)
    {
        _logsInfo.Add(logInfo);
        switch (logInfo.LogType) {
        case BluLogType.Normal:
            _qtNormalLogs++;
            break;
        case BluLogType.Warning:
            _qtWarningLogs++;
            break;
        case BluLogType.Error:
            _qtErrorLogs++;
            if (_isPauseOnError)
                Debug.Break();
            break;
        }
    }

    #endregion


}

}

#endif