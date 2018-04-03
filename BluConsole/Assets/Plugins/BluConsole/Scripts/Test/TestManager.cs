using BluConsole.Core;
using UnityEngine;


namespace BluConsole.Test
{

    public class TestManager : MonoBehaviour
    {

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Test1.BigCallStack(RandomMessage, RandomLogType);
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                GenerateLotOfLogs();
            }
        }

        private void GenerateLotOfLogs()
        {
            int qt = Random.Range(100, 200);
            for (int i = 0; i < qt; i++)
            {
                int random = Random.Range(1, 7);
                if (random == 1)
                    Test1.LogLevelOne(RandomMessage, RandomLogType);
                else if (random == 2)
                    Test1.LogLevelTwo(RandomMessage, RandomLogType);
                else if (random == 3)
                    Test1.LogLevelThree(RandomMessage, RandomLogType);
                else if (random == 4)
                    Test1.LogLevelFour(RandomMessage, RandomLogType);
                else if (random == 5)
                    Test1.LogLevelFive(RandomMessage, RandomLogType);
                else if (random == 6)
                    Test1.LogLevelSix(RandomMessage, RandomLogType);
            }
        }

        private BluLogType RandomLogType
        {
            get
            {
                int random = Random.Range(0, 3);
                if (random == 0)
                    return BluLogType.Normal;
                else if (random == 1)
                    return BluLogType.Warning;
                else
                    return BluLogType.Error;
            }
        }

        private string RandomMessage
        {
            get
            {
                int random = Random.Range(0, 101);
                if (random >= 0 && random < 40)
                    return "Small Message";
                else if (random >= 40 && random < 90)
                    return "Crazy Message... Crazy Message...";

                return BigString;
            }
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
        public static void Log(string message, BluLogType type)
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
