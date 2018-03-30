using System;

namespace Svelto.ECS
{
    public class TypeSafeFasterListForECSException : Exception
    {
        public TypeSafeFasterListForECSException(Exception exception):base("Trying to add an Entity View with the same ID more than once", exception)
        {}
    }
}