using System;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HashNameAttribute:Attribute
    {
        public HashNameAttribute(string name)
        {
            _name = name;
        }
        
        internal readonly string _name;
    }
}