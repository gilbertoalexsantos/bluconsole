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
using System.Reflection;


namespace BluConsole.Core.UnityLoggerApi
{

public static class LogEntries
{

    public static void Clear()
    {
        var method = GetMethod("Clear");
        method.Invoke(null, null);
    }

    public static BluLog GetCompleteLog(
        int row)
    {
        var emptyLog = LogEntry.GetNewEmptyObject;
        GetEntryInternal(row, emptyLog);
        return LogEntry.GetBluLog(emptyLog);
    }

    public static BluLog GetSimpleLog(
        int row)
    {
        int mode = 0;
        string message = "";
        GetFirstTwoLinesEntryTextAndModeInternal(row, ref mode, ref message);

        BluLog log = new BluLog();
        log.SetMessage(message);
        log.SetMode(mode);
        return log;
    }

    public static int GetEntryCount(
        int row)
    {
        var method = GetMethod("GetEntryCount");
        return (int)method.Invoke(null, new object[] { row });
    }

    public static int GetCount()
    {
        var method = GetMethod("GetCount");
        return (int)method.Invoke(null, null);
    }

    public static void GetCountsByType(
        ref int error,
        ref int warning,
        ref int normal)
    {
        var method = GetMethod("GetCountsByType");

        int _error = 0, _warning = 0, _normal = 0;
        var parameters = new object[] {
            _error,
            _warning,
            _normal
        };
        method.Invoke(null, parameters);

        error = (int)parameters[0];
        warning = (int)parameters[1];
        normal = (int)parameters[2];
    }

    public static int StartGettingEntries()
    {
        var method = GetMethod("StartGettingEntries");
        return (int)method.Invoke(null, null);
    }

    public static void EndGettingEntries()
    {
        var method = GetMethod("EndGettingEntries");
        method.Invoke(null, null);
    }

    private static bool GetEntryInternal(
        int row,
        object output)
    {
        var method = GetMethod("GetEntryInternal");
        return (bool)method.Invoke(null, new object[] {
                row,
                output
            });
    }

    private static void GetFirstTwoLinesEntryTextAndModeInternal(
        int row,
        ref int mode,
        ref string text)
    {
        var method = GetMethod("GetFirstTwoLinesEntryTextAndModeInternal");

        int _mode = 0;
        string _text = "";
        var parameters = new object[] {
            row,
            _mode,
            _text
        };
        method.Invoke(null, parameters);

        mode = (int)parameters[1];
        text = (string)parameters[2];
    }

    private static MethodInfo GetMethod(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntriesType.GetMethod(key, DefaultFlags));
        return CachedReflection.Get<MethodInfo>(key);
    }

    private static PropertyInfo GetProperty(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntriesType.GetProperty(key, DefaultFlags));
        return CachedReflection.Get<PropertyInfo>(key);
    }

    private static Type LogEntriesType
    {
        get
        {
            if (!CachedReflection.Has("LogEntries"))
                CachedReflection.Cache("LogEntries", Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll"));
            return CachedReflection.Get<Type>("LogEntries");
        }
    }

    private static BindingFlags DefaultFlags
    {
        get
        {
            return BindingFlags.Static | BindingFlags.Public;
        }
    }


}

}
