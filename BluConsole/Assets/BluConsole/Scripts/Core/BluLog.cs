using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BluConsole.Core
{

[Serializable]
public class BluLog
{

    public string Message { get; private set; }

    public string MessageLower { get; private set; }

    public string File { get; private set; }

    public int Line { get; private set; }

    public int Mode { get; private set; }

    public int InstanceID { get; private set; }

    public List<BluLogFrame> StackTrace { get; private set; }

    public void SetMessage(
        string condition)
    {
        int index = 0;
        while (condition[index++] != '\n');
        Message = condition.Substring(0, index-1);
        MessageLower = Message.ToLower();
    }

    public void SetFile(
        string file)
    {
        File = file;
    }

    public void SetLine(
        int line)
    {
        Line = line;
    }

    public void SetMode(
        int mode)
    {
        Mode = mode;
    }

    public void SetStackTrace(string condition)
    {
        StackTrace = new List<BluLogFrame>();

        var splits = condition.Split('\n');
        for (int i = 1; i < splits.Length; i++)
        {
            if (string.IsNullOrEmpty(splits[i]))
                continue;
            StackTrace.Add(new BluLogFrame(splits[i]));
        }
    }

    public void SetInstanceID(int instanceID)
    {
        InstanceID = instanceID;
    }

    public override string ToString()
    {
        return string.Format("[BluLog: Message={0}, File={1}, Line={2}, Mode={3}, StackTrace={4}]", 
                             Message, File, Line, Mode, StackTrace);
    }

}

}
