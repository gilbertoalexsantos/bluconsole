using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace BluConsole
{

public class LogInfoComparer : IEqualityComparer<LogInfo>
{
    
    #region IEqualityComparer implementation

    public bool Equals(LogInfo x,
                       LogInfo y)
    {
        return x.RawMessage == y.RawMessage;
    }

    public int GetHashCode(LogInfo obj)
    {
        return obj.RawMessage.GetHashCode();
    }

    #endregion
    
}


[Serializable]
public class LogInfo
{

    [SerializeField] private string _rawMessage;
    [SerializeField] private string _message;
    [SerializeField] private List<LogStackFrame> _callStack;
    [SerializeField] private BluLogType _logType;
    [SerializeField] private bool _isCompilerError;

    public string RawMessage
    {
        get
        {
            return _rawMessage;
        }
    }

    public string Message
    {
        get
        {
            return _message;
        }
    }

    public List<LogStackFrame> CallStack
    {
        get
        {
            return _callStack;
        }
    }

    public BluLogType LogType
    {
        get
        {
            return _logType;
        }
    }

    public bool IsCompilerError
    {
        get
        {
            return _isCompilerError;
        }
    }

    public LogInfo(string rawMessage,
                   string message,
                   List<LogStackFrame> callStack,
                   BluLogType logType,
                   bool isCompilerError)
    {
        _rawMessage = rawMessage;
        _message = message;
        _callStack = callStack;
        _logType = logType;
        _isCompilerError = isCompilerError;
    }

}

}
