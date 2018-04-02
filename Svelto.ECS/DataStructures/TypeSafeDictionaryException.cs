using System;

namespace Svelto.ECS
{
    public class TypeSafeDictionaryException : Exception
    {
        public TypeSafeDictionaryException(Exception exception) : base("trying to add an EntityView with the same ID more than once", exception)
        {
        }
    }
}