using BluConsole.Core;


namespace BluConsole.Test
{

    public static class Test6
    {

        public static void LogLevelSix(string message, BluLogType type = BluLogType.Normal)
        {
            TestManager.Log(message, type);
        }

    }

}
