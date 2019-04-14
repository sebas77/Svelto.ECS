using System;

namespace Svelto.ECS.Internal
{
    class EntityGroupNotFoundException : Exception
    {
        public EntityGroupNotFoundException(uint groupId, Type type)
            : base("entity group not found ".FastConcat(type.ToString()))
        {
        }
    }
}