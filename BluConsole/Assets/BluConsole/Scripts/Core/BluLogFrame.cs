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


namespace BluConsole.Core
{

[Serializable]
public class BluLogFrame
{

    public string FrameInformation { get; private set; }

    public string File { get; private set; }

    public int Line { get; private set; }

    public BluLogFrame(
        string frameInformation)
    {
        FrameInformation = frameInformation;

        int index = frameInformation.IndexOf("(at");
        if (index == -1)
            return;

        index += 4;

        int begFile = index;
        while (index < frameInformation.Length && frameInformation[index] != ':')
            index++;
        int endFile = index-1;

        index += 1;

        int begLine = index;
        while (index < frameInformation.Length && frameInformation[index] != ')')
            index++;
        int endLine = index-1;

        int line;
        if (index+1 != frameInformation.Length || 
                !int.TryParse(frameInformation.Substring(begLine, endLine - begLine + 1), out line))
        {
            File = "";
            Line = 0;
            return;
        }

        File = frameInformation.Substring(begFile, endFile - begFile + 1);
        Line = line;
    }

    public override string ToString()
    {
        return string.Format("[BluLogFrame: FrameInformation={0}, File={1}, Line={2}]", 
                                 FrameInformation, File, Line);
    }
}

}
