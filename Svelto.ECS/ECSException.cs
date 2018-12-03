using System;

namespace Svelto.ECS.Internal
{
    class ECSException : Exception
    {
        public ECSException(string message):base(message)
        {}
        
        public ECSException(string message, Exception innerE):base(message, innerE)
        {}
    }
}