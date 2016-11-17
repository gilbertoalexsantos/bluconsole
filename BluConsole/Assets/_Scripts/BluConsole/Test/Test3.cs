using UnityEngine;
using System.Collections;
using BluConsole;

public static class Test3 {

    public static void LogLevelThree(string message, BluLogType type = BluLogType.Normal)
    {
        TestManager.Log(message, type);
    }

    public static void LogLevelFour(string message, BluLogType type = BluLogType.Normal)
    {
        Test4.LogLevelFour(message, type);
    }

    public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
    {
        Test4.LogLevelFive(message, type);
    }

}
