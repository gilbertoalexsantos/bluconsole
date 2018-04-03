using BluConsole.Core;


namespace BluConsole.Test
{

    public static class Test5
    {

        public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
        {
            TestManager.Log(message, type);
        }

        public static void LogLevelSix(string message, BluLogType type = BluLogType.Normal)
        {
            Test6.LogLevelSix(message, type);
        }

        public static void Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(string message, BluLogType type = BluLogType.Normal)
        {
            Test6.Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(message, type);
        }

    }

}
