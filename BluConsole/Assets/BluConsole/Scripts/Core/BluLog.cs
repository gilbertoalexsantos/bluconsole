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
        if (string.IsNullOrEmpty(condition))
            return;
        
        int index = 0;
        while (index < condition.Length && condition[index++] != '\n');
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

        if (string.IsNullOrEmpty(condition))
            return;

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

    public void FilterStackTrace(List<string> prefixs)
    {
        var newStackTrace = new List<BluLogFrame>(StackTrace.Count);
        foreach (var frame in StackTrace)
        {
            bool hasPrefix = false;
            foreach (var prefix in prefixs)
            {
                if (frame.FrameInformation.StartsWith(prefix))
                {
                    hasPrefix = true;
                    break;
                }
            }
            if (!hasPrefix)
                newStackTrace.Add(frame);
        }
        StackTrace = newStackTrace;
    }

    public override string ToString()
    {
        return string.Format("[BluLog: Message={0}, File={1}, Line={2}, Mode={3}, StackTrace={4}]", 
                             Message, File, Line, Mode, StackTrace);
    }

}

}
