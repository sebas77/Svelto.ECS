using System;

namespace Svelto.ECS
{
    public class ECSException : Exception
    {
        public ECSException(string message):base("<color=red>".FastConcat(message, "</color>"))
        {}
        
        public ECSException(string message, Exception innerE):base("<color=red>".FastConcat(message, "</color>"), innerE)
        {}
    }
}