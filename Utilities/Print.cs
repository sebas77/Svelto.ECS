using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

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
}

namespace Utility
{
    public static class Console
    {
        static StringBuilder _stringBuilder = new StringBuilder(256);

        public static void Log(string txt)
        {
            Debug.Log(txt);
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
                
            Debug.LogError(toPrint);
        }

        public static void LogException(Exception e)
        {
            LogException(e, null);
        }

        public static void LogException(Exception e, UnityEngine.Object obj)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                _stringBuilder.Length = 0;
                _stringBuilder.Append("-!!!!!!-> ").Append(e);

                toPrint = _stringBuilder.ToString();
            }

            Exception ex = new Exception(e.ToString());

            Debug.Log(toPrint);
            Debug.LogException(ex, obj); 
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

            Debug.LogWarning(toPrint);
        }

        /// <summary>
        /// This function should never be used explicitly
        /// </summary>
        /// <param name="txt"></param>
        public static void SystemLog(string txt)
        {
            string toPrint;

            lock (_stringBuilder)
            {
                string currentTimeString = DateTime.UtcNow.ToLongTimeString(); //ensure includes seconds
                string processTimeString = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString();

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
            Debug.Log(toPrint);
#endif
        }
    }
}
