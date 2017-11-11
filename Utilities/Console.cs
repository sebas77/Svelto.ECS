using System;
#if NETFX_CORE
using Windows.System.Diagnostics;
#else
using System.Diagnostics;
#endif
using System.Text;

namespace Utility
{
    public static class Console
    {
        static StringBuilder _stringBuilder = new StringBuilder(256);

        public static ILogger logger;

        public static volatile bool BatchLog = false;

        //Hack, have to find the right solution
        public static Action<Exception, string, string> onException;

        static Console()
        {
#if UNITY_5_3_OR_NEWER || UNITY_5
            logger = new SlowLoggerUnity();
            onException = (e, message, stack) =>
                {
                    UnityEngine.Debug.LogException(e, null);
                };
#else
            logger = new SimpleLogger();
#endif
        }

        public static void Log(string txt)
        {
            logger.Log(txt);
        }

        public static void LogError(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            logger.Log(toPrint, null, LogType.Error);
        }

        public static void LogError(string txt, string stack)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            logger.Log(toPrint, stack, LogType.Error);
        }

        public static void LogException(Exception e)
        {
            string toPrint;
            string stackTrace;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ").Append(e.Message);

                stackTrace = e.StackTrace;

                if (e.InnerException != null)
                {
                    e = e.InnerException;

                    _stringBuilder.Append(" Inner Message: ").Append(e.Message).Append(" Inner Stacktrace:")
                        .Append(e.StackTrace);

                    stackTrace = e.StackTrace;
                }

                toPrint = _stringBuilder.ToString();
            }

            if (onException != null)
                onException(e, toPrint, stackTrace);
        }

        public static void LogWarning(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("------> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            logger.Log(toPrint, null, LogType.Warning);
        }

        /// <summary>
        /// Use this function if you don't want the message to be batched
        /// </summary>
        /// <param name="txt"></param>
        public static void SystemLog(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
#if NETFX_CORE
                string currentTimeString = DateTime.UtcNow.ToString("dd/mm/yy hh:ii:ss");
                string processTimeString = (DateTime.UtcNow - ProcessDiagnosticInfo.GetForCurrentProcess().ProcessStartTime.DateTime).ToString();
#else
                string currentTimeString = DateTime.UtcNow.ToLongTimeString(); //ensure includes seconds
                string processTimeString = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString();
#endif

                _stringBuilder.Length = 0;
                _stringBuilder.Append("[").Append(currentTimeString);
                _stringBuilder.Append("][").Append(processTimeString);
                _stringBuilder.Length = _stringBuilder.Length - 3; //remove some precision that we don't need
                _stringBuilder.Append("] ").AppendLine(txt);

                toPrint = _stringBuilder.ToString();
            }

#if !UNITY_EDITOR
#if !NETFX_CORE
            System.Console.WriteLine(toPrint);
#else
            //find a way to adopt a logger externally, if this is still needed
#endif
#else
            UnityEngine.Debug.Log(toPrint);
#endif
        }
    }
}