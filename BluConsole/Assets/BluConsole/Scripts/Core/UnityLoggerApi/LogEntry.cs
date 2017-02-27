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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BluConsole.Core;


namespace BluConsole.Core.UnityLoggerApi
{

/*
 * Fields:
 * string condition
 * int errorNum
 * string file
 * int line
 * int mode
 * int instanceID
 * int identifier
 * int isWorldPlaying
 */
public static class LogEntry
{

    private static object _cacheLogEntryInstance = null;

    public static object GetNewEmptyObject 
    { 
        get 
        { 
            return _cacheLogEntryInstance ?? (_cacheLogEntryInstance = Activator.CreateInstance(LogEntryType)); 
        } 
    }

    public static BluLog GetBluLog(
        object obj)
    {
        var log = new BluLog();

        var logEntryType = LogEntryType;

        var condition = (string)GetField("condition", logEntryType).GetValue(obj);
        log.SetMessage(condition);
        log.SetStackTrace(condition);

        var file = (string)GetField("file", logEntryType).GetValue(obj);
        log.SetFile(file);

        var line = (int)GetField("line", logEntryType).GetValue(obj);
        log.SetLine(line);

        var mode = (int)GetField("mode", logEntryType).GetValue(obj);
        log.SetMode(mode);

        var instanceID = (int)GetField("instanceID", logEntryType).GetValue(obj);
        log.SetInstanceID(instanceID);

        return log;
    }

    private static FieldInfo GetField(
        string key,
        Type type)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, type.GetField(key));
        return CachedReflection.Get<FieldInfo>(key);
    }

    private static MethodInfo GetMethod(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntryType.GetMethod(key, DefaultFlags));
        return CachedReflection.Get<MethodInfo>(key);
    }

    private static PropertyInfo GetProperty(
        string key)
    {
        if (!CachedReflection.Has(key))
            CachedReflection.Cache(key, LogEntryType.GetProperty(key, DefaultFlags));
        return CachedReflection.Get<PropertyInfo>(key);
    }

    private static Type LogEntryType
    {
        get
        {
            if (!CachedReflection.Has("LogEntry"))
                CachedReflection.Cache("LogEntry", Type.GetType("UnityEditorInternal.LogEntry,UnityEditor.dll"));
            return CachedReflection.Get<Type>("LogEntry");
        }
    }

    private static BindingFlags DefaultFlags
    {
        get
        {
            return BindingFlags.Public;
        }
    }

}

}
