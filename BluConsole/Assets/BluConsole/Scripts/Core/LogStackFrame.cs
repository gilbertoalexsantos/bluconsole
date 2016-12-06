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
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


namespace BluConsole.Core
{

[Serializable]
public class LogStackFrame
{

    private static readonly string REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER =
        @"(.*):(.*)\s*\(.*\(at\s*(.*):(\d+)";
    private static readonly string REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER_WIHTOUT_COLON =
        @"(.*)\s*\(.*\(at\s*(.*):(\d+)";
    private static readonly string REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER =
        @"(.*):(.*)\s*\(.*\)";

    [SerializeField] private string _className;
    [SerializeField] private string _methodName;
    [SerializeField] private string _formattedMethodName;
    [SerializeField] private string _filePath;
    [SerializeField] private int _line;

    public string ClassName
    {
        get
        {
            return _className;
        }
    }

    public string MethodName
    {
        get
        {
            return _methodName;
        }
    }

    public string FilePath
    {
        get
        {
            return _filePath;
        }
    }

    public int Line
    {
        get
        {
            return _line;
        }
    }

    public string FormattedMethodName
    {
        get
        {
            return _formattedMethodName;
        }
    }

    public LogStackFrame(
        string className,
        string methodName,
        string fileRelativePath,
        int line)
    {
        _className = className;
        _methodName = methodName;
        _filePath = fileRelativePath;
        _line = line;

        if (!String.IsNullOrEmpty(_filePath))
        {
            var startSubName = _filePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            var formattedPath = startSubName >= 0 ? _filePath.Substring(startSubName) : _filePath;
            _formattedMethodName = String.Format("{0}.{1} (at {2}:{3})",
                                                 _className,
                                                 _methodName,
                                                 formattedPath,
                                                 _line);
        }
        else
        {
            _formattedMethodName = String.Format("{0}.{1}", _className, _methodName);
        }
    }

    public static LogStackFrame Create(
        StackFrame frame)
    {
        MethodBase method = frame.GetMethod();

        return new LogStackFrame(className: method.DeclaringType.ToString(),
                                 methodName: method.Name,
                                 fileRelativePath: frame.GetFileName(),
                                 line: frame.GetFileLineNumber());
    }

    public static LogStackFrame Create(
        string unityStackFrame)
    {
        MatchCollection matchWithLineNumber =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER);

        MatchCollection matchWithLineNumberWithoutColon =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER_WIHTOUT_COLON);

        MatchCollection matchWithoutLineNumber =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER);

        if (matchWithLineNumber.Count > 0)
        {
            return new LogStackFrame(className: matchWithLineNumber[0].Groups[1].Value,
                                     methodName: matchWithLineNumber[0].Groups[2].Value,
                                     fileRelativePath: matchWithLineNumber[0].Groups[3].Value,
                                     line: Convert.ToInt32(matchWithLineNumber[0].Groups[4].Value));
        }
        else if (matchWithLineNumberWithoutColon.Count > 0)
        {
            string classNameWithMethodName = matchWithLineNumberWithoutColon[0].Groups[1].Value.Replace(" ", "");

            string className = classNameWithMethodName;
            string methodName = classNameWithMethodName;

            var classNameWithMethodNameByPoint = classNameWithMethodName.Split('.');
            if (classNameWithMethodNameByPoint.Length > 0)
            {
                className = classNameWithMethodNameByPoint[0];
                for (int i = 1; i + 1 < classNameWithMethodNameByPoint.Length; i++)
                    className += "." + classNameWithMethodNameByPoint[i];
                methodName = classNameWithMethodNameByPoint[classNameWithMethodNameByPoint.Length - 1];
            }

            return new LogStackFrame(className: className,
                                     methodName: methodName,
                                     fileRelativePath: matchWithLineNumberWithoutColon[0].Groups[2].Value,
                                     line: Convert.ToInt32(matchWithLineNumberWithoutColon[0].Groups[3].Value));
        }
        else if (matchWithoutLineNumber.Count > 0)
        {
            return new LogStackFrame(className: matchWithoutLineNumber[0].Groups[1].Value,
                                     methodName: matchWithoutLineNumber[0].Groups[2].Value,
                                     fileRelativePath: matchWithoutLineNumber[0].Groups[3].Value,
                                     line: -1);
        }
        else
        {
            throw new Exception("Can't create it. Call CanGetInformation to check " +
                "if can create an instance from an unityStackFrame");
        }
    }

    public static bool CanGetInformation(
        string unityStackFrame)
    {
        MatchCollection matchWithLineNumber =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER);

        MatchCollection matchWithLineNumberWithoutColon =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER_WIHTOUT_COLON);

        MatchCollection matchWithoutLineNumber =
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER);

        return
            matchWithLineNumber.Count > 0 ||
        matchWithLineNumberWithoutColon.Count > 0 ||
        matchWithoutLineNumber.Count > 0;
    }

}

}
