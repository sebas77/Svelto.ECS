namespace Utility
{
    public enum LogType
    {
        Log,
        Exception,
        Warning,
        Error
    }
    public interface ILogger
    {
        void Log (string txt, string stack = null, LogType type = LogType.Log);
    }
}