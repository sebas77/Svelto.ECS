using System;

namespace Svelto.ECS
{
    public class EntitiesDBException : Exception
    {
        public EntitiesDBException(string message):base(message)
        {}
    }
}