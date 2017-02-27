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


using BluConsole;
using BluConsole.Core;
using BluConsole.Core.UnityLoggerApi;
using UnityEngine;
using System;


namespace BluConsole.Test
{

public class TestManager : MonoBehaviour
{

    public float LOG_DELAY = 0.3f;

    private float _time = 0f;

    private void Update()
    {
        int h = 5;
        if (Input.GetKeyDown(KeyCode.A))
        {
            Test1.LogLevelOne(RandomMessage(), RandomLogType());
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Test1.LogLevelTwo(RandomMessage(), RandomLogType());
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Test1.LogLevelThree(RandomMessage(), RandomLogType());
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Test1.LogLevelFour(RandomMessage(), RandomLogType());
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            Test1.LogLevelFive(RandomMessage(), RandomLogType());
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            GenerateLotOfLogs();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            int? t = null;
            Debug.LogError(t);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log(BigString);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
//            bool t = 1 & 0;
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            foreach (var en in Enum.GetValues(typeof(ConsoleWindowMode)))
            {
                var enn = (ConsoleWindowMode)en;
                if (((int)enn & 16640) != 0)
                {
                    Debug.LogError("Has this mode: " + enn.ToString());
                }
            }
        }

        _time += Time.deltaTime;
        if (_time >= LOG_DELAY)
        {
            _time = 0f;
            for (int i = 0; i < 30; i++)
            {
//                Test1.LogLevelFive("Update!!!", RandomLogType());
            }
        }
    }

    private void GenerateLotOfLogs()
    {
        int qt = UnityEngine.Random.Range(100, 200);
        for (int i = 0; i < qt; i++)
        {
            int random = UnityEngine.Random.Range(1, 6);
            if (random == 1)
                Test1.LogLevelOne(RandomMessage(), RandomLogType());
            else if (random == 2)
                Test1.LogLevelTwo(RandomMessage(), RandomLogType());
            else if (random == 3)
                Test1.LogLevelThree(RandomMessage(), RandomLogType());
            else if (random == 4)
                Test1.LogLevelFour(RandomMessage(), RandomLogType());
            else
                Test1.LogLevelFive(RandomMessage(), RandomLogType());
        }
    }

    private BluLogType RandomLogType()
    {
        int random = UnityEngine.Random.Range(0, 3);
        if (random == 0)
            return BluLogType.Normal;
        else if (random == 1)
            return BluLogType.Warning;
        else
            return BluLogType.Error;
    }

    private string RandomMessage()
    {
        int random = UnityEngine.Random.Range(0, 101);
        if (random >= 0 && random < 40)
            return "Small Message";
        else if (random >= 40 && random < 90)
            return "Crazy Message... Crazy Message...";

        return BigString;
    }

    private string BigString
    {
        get
        {
            return "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        }
    }         

    [StackTraceIgnore]
    public static void Log(
        string message,
        BluLogType type)
    {
        if (type == BluLogType.Normal)
            Debug.Log(message);
        else if (type == BluLogType.Warning)
            Debug.LogWarning(message);
        else if (type == BluLogType.Error)
            Debug.LogError(message);
    }

}

}
