namespace Utility
{
    public class SimpleLogger : ILogger
    {
        public void Log(string txt, string stack = null, LogType type = LogType.Log)
        {
            switch (type)
            {
                case LogType.Log:
                    Console.SystemLog(stack != null ? txt.FastConcat(stack) : txt);
                    break;
                case LogType.Exception:
                    Console.SystemLog("Log of exceptions not supported");
                    break;
                case LogType.Warning:
                    Console.SystemLog(stack != null ? txt.FastConcat(stack) : txt);
                    break;
                case LogType.Error:
                    Console.SystemLog(stack != null ? txt.FastConcat(stack) : txt);
                    break;
            }
        }
    }
}