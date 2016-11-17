using UnityEngine;
using System.Collections;
using BluConsole;

public static class Test5 {

    public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
    {
        TestManager.Log(message, type);
    }

}
