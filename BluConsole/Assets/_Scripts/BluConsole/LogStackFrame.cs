using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;


namespace BluConsole
{

[Serializable]
public class LogStackFrame
{

    private static readonly string REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER = @"(.*):(.*)\s*\(.*\(at\s*(.*):(\d+)";
    private static readonly string REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER = @"(.*):(.*)\s*\(.*\)";

    [SerializeField] private string _className;
    [SerializeField] private string _methodName;
    [SerializeField] private string _fileRelativePath;
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

    public string FileRelativePath
    {
        get
        {
            return _fileRelativePath;
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
            var formattedFileRelativePath = _fileRelativePath;
            if (!String.IsNullOrEmpty(_fileRelativePath)) {
                var startSubName = _fileRelativePath.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);

                if (startSubName > 0)
                    formattedFileRelativePath = _fileRelativePath.Substring(startSubName);
            }
            return String.Format("{0}.{1} (at {2}:{3})", 
                                 _className, 
                                 _methodName, 
                                 formattedFileRelativePath, 
                                 _line);
        }
    }

    public LogStackFrame(string className,
                         string methodName,
                         string fileRelativePath,
                         int line)
    {
        _className = className;
        _methodName = methodName;
        _fileRelativePath = fileRelativePath;
        _line = line;
    }

    public static LogStackFrame Create(StackFrame frame)
    {
        MethodBase method = frame.GetMethod();

        return new LogStackFrame(className: method.DeclaringType.Name,
                                 methodName: method.Name,
                                 fileRelativePath: frame.GetFileName(),
                                 line: frame.GetFileLineNumber());
    }

    public static LogStackFrame Create(string unityStackFrame)
    {
        MatchCollection matchWithLineNumber = 
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER);
        MatchCollection matchWithoutLineNumber = 
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER);

        if (matchWithLineNumber.Count > 0) {
            return new LogStackFrame(className: matchWithLineNumber[0].Groups[1].Value,
                                     methodName: matchWithLineNumber[0].Groups[2].Value,
                                     fileRelativePath: matchWithLineNumber[0].Groups[3].Value,
                                     line: Convert.ToInt32(matchWithLineNumber[0].Groups[4].Value));
        } else if (matchWithoutLineNumber.Count > 0) {
            return new LogStackFrame(className: matchWithoutLineNumber[0].Groups[1].Value,
                                     methodName: matchWithoutLineNumber[0].Groups[2].Value,
                                     fileRelativePath: matchWithoutLineNumber[0].Groups[3].Value,
                                     line: -1);
        } else {
            throw new Exception("Can't create it. Call CanGetInformation to check " +
            "if can create an instance from an unityStackFrame");
        }
    }

    public static bool CanGetInformation(string unityStackFrame)
    {
        MatchCollection matchWithLineNumber = 
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITH_LINE_NUMBER);
        MatchCollection matchWithoutLineNumber = 
            Regex.Matches(unityStackFrame, REGEX_UNITY_STACK_TRACE_WITHOUT_LINE_NUMBER);
        return matchWithLineNumber.Count > 0 || matchWithoutLineNumber.Count > 0;
    }

}

}
