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
using System.Runtime.Remoting.Messaging;


namespace BluConsole.Core.UnityLoggerApi
{

public static class UnityLoggerServer
{

    public static void Clear()
    {
        LogEntries.Clear();
    }

    public static BluLog GetCompleteLog(
        int row)
    {
        return LogEntries.GetCompleteLog(row);
    }

    public static BluLog GetSimpleLog(
        int row)
    {
        return LogEntries.GetSimpleLog(row);
    }

    public static int GetLogCount(
        int row)
    {
        return LogEntries.GetEntryCount(row);
    }

    public static int StartGettingLogs()
    {
        return LogEntries.StartGettingEntries();
    }

    public static void StopGettingsLogs()
    {
        LogEntries.EndGettingEntries();
    }

    public static int GetCount()
    {
        return LogEntries.GetCount();
    }

    public static void GetCount(
        ref int normal,
        ref int warning,
        ref int error)
    {
        LogEntries.GetCountsByType(ref error, ref warning, ref normal);
    }

    public static bool IsDebugError(
        int mode)
    {
        return ConsoleWindow.IsDebugError(mode);
    }

    public static bool HasFlag(
        ConsoleWindowFlag flag)
    {
        return ConsoleWindow.HasFlag(flag);
    }

    public static void SetFlag(
        ConsoleWindowFlag flag,
        bool active)
    {
        ConsoleWindow.SetFlag(flag, active);
    }

    public static bool HasMode(
        int mode,
        ConsoleWindowMode modeToCheck)
    {
        return ConsoleWindow.HasMode(mode, modeToCheck);
    }

}

}
