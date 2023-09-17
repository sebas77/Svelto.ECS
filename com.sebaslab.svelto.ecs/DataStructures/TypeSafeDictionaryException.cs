using System;

namespace Svelto.ECS
{
    public class TypeSafeDictionaryException : Exception
    {
        public TypeSafeDictionaryException(string message, Exception exception) : 
            base(message, exception)
        {
        }
        
        public TypeSafeDictionaryException(string message) : 
                base(message)
        {
        }
    }
}