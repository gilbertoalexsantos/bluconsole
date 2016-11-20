using UnityEngine;
using System.Collections;
using BluConsole;

namespace BluConsole.Test {

public static class Test5 {

    public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
    {
        TestManager.Log(message, type);
    }

}

}
