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


using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace BluConsole.Core
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
