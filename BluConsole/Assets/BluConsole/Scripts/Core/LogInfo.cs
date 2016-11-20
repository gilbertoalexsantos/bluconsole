using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace BluConsole
{

public class LogInfoComparer : IEqualityComparer<LogInfo>
{

    #region IEqualityComparer implementation


    public bool Equals(
        LogInfo x,
        LogInfo y)
    {
        return x.Identifier == y.Identifier;
    }

    public int GetHashCode(
        LogInfo obj)
    {
        return obj.Identifier.GetHashCode();
    }


    #endregion

}


[Serializable]
public class LogInfo
{

    [SerializeField] private string _identifier;
    [SerializeField] private string _rawMessage;
    [SerializeField] private string _message;
    [SerializeField] private List<LogStackFrame> _callStack;
    [SerializeField] private BluLogType _logType;
    [SerializeField] private bool _isCompilerMessage;

    public string Identifier
    {
        get
        {
            return _identifier;
        }
    }

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

    public bool IsCompileMessage
    {
        get
        {
            return _isCompilerMessage;
        }
    }

    public LogInfo(
        string rawMessage,
        string message,
        List<LogStackFrame> callStack,
        BluLogType logType,
        bool isCompileMessage)
    {
        _rawMessage = rawMessage;
        _message = message;
        _callStack = callStack;
        _logType = logType;
        _isCompilerMessage = isCompileMessage;

        StringBuilder identifier = new StringBuilder();
        identifier.Append(_rawMessage.GetHashCode().ToString());
        identifier.Append("$");
        identifier.Append(_logType.ToString());
        identifier.Append("$");
        identifier.Append(callStack.Count);
        foreach (LogStackFrame frame in callStack)
        {
            identifier.Append("$");
            identifier.Append(frame.ClassName);
            identifier.Append("$");
            identifier.Append(frame.Line.ToString());
        }

        _identifier = identifier.ToString();
    }

}

}
