using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class FastConcatUtility
{
    static readonly StringBuilder _stringBuilder = new StringBuilder(256);

    public static string FastConcat(this string str1, string str2)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);

            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);
            _stringBuilder.Append(str4);


            return _stringBuilder.ToString();
        }
    }

    public static string FastConcat(this string str1, string str2, string str3, string str4, string str5)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            _stringBuilder.Append(str1);
            _stringBuilder.Append(str2);
            _stringBuilder.Append(str3);
            _stringBuilder.Append(str4);
            _stringBuilder.Append(str5);

            return _stringBuilder.ToString();
        }
    }

    public static string FastJoin(this string[] str)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            for (int i = 0; i < str.Length; i++)
                _stringBuilder.Append(str[i]);

            return _stringBuilder.ToString();
        }
    }

    public static string FastJoin(this string[] str, string str1)
    {
        lock (_stringBuilder)
        {
            _stringBuilder.Length = 0;

            for (int i = 0; i < str.Length; i++)
                _stringBuilder.Append(str[i]);

            _stringBuilder.Append(str1);

            return _stringBuilder.ToString();
        }
    }
}

namespace Utility
{
    public interface ILogger
    {
        void Log (string txt, string stack = null, LogType type = LogType.Log);
    }

    public class SlowLogger : ILogger
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

    public static class Console
    {
        static StringBuilder _stringBuilder = new StringBuilder(256);

        public static ILogger logger = new SlowLogger();
        public static volatile bool BatchLog = false;

        public static void Log(string txt)
        {
            logger.Log(txt);
        }

        public static void LogError(string txt, bool showCurrentStack = true)
        {
            string toPrint;
        
            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ");
                _stringBuilder.Append(txt);

                toPrint = _stringBuilder.ToString();
            }

            logger.Log(toPrint, showCurrentStack == true ? new StackTrace().ToString() : null, LogType.Error);
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
            LogException(e, null);
        }

        public static void LogException(Exception e, UnityEngine.Object obj)
        {
            UnityEngine.Debug.LogException(e, obj);
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
                string currentTimeString = DateTime.UtcNow.ToLongTimeString(); //ensure includes seconds
                string processTimeString = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime).ToString();

                _stringBuilder.Length = 0;
                _stringBuilder.Append("[").Append(currentTimeString);
                _stringBuilder.Append("][").Append(processTimeString);
                _stringBuilder.Length = _stringBuilder.Length - 3; //remove some precision that we don't need
                _stringBuilder.Append("] ").AppendLine(txt);

                toPrint = _stringBuilder.ToString();
            }

#if !UNITY_EDITOR
            System.Console.WriteLine(toPrint);
#else
            UnityEngine.Debug.Log(toPrint);
#endif
        }
    }
}
