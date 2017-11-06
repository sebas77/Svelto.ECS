#if UNITY_5_3_OR_NEWER || UNITY_5
namespace Utility
{
    public class SlowLoggerUnity : ILogger
    {
        public void Log(string txt, string stack = null, LogType type = LogType.Log)
        {
            switch (type)
            {
                case LogType.Log:
                    UnityEngine.Debug.Log(stack != null ? txt.FastConcat(stack) : txt);
                    break;
                case LogType.Exception:
                    UnityEngine.Debug.LogError("Log of exceptions not supported");
                    break;
                case LogType.Warning:
                    UnityEngine.Debug.LogWarning(stack != null ? txt.FastConcat(stack) : txt);
                    break;
                case LogType.Error:
                    UnityEngine.Debug.LogError(stack != null ? txt.FastConcat(stack) : txt);
                    break;
            }
        }
    }
}
#endif