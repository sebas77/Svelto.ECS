using System;

namespace Svelto.ECS
{
    public class TypeSafeDictionaryException : Exception
    {
        public TypeSafeDictionaryException(Exception exception) : base(exception.Message, exception)
        {
        }
    }
}