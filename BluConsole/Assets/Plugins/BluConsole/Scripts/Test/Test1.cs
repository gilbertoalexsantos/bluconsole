using BluConsole.Core;


namespace BluConsole.Test
{

    public static class Test1
    {

        public static void LogLevelOne(string message, BluLogType type = BluLogType.Normal)
        {
            TestManager.Log(message, type);
        }

        public static void LogLevelTwo(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.LogLevelTwo(message, type);
        }

        public static void LogLevelThree(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.LogLevelThree(message, type);
        }

        public static void LogLevelFour(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.LogLevelFour(message, type);
        }

        public static void LogLevelFive(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.LogLevelFive(message, type);
        }

        public static void LogLevelSix(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.LogLevelSix(message, type);
        }

        public static void BigCallStack(string message, BluLogType type = BluLogType.Normal)
        {
            Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(message, type);
        }

        public static void Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(string message, BluLogType type = BluLogType.Normal)
        {
            Test2.Loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog(message, type);
        }

    }

}
