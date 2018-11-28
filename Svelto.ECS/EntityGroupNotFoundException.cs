using System;

namespace Svelto.ECS.Internal
{
    class EntityGroupNotFoundException : Exception
    {
        public EntityGroupNotFoundException(int groupId, Type type)
            : base("entity group not found ".FastConcat(type.ToString()))
        {
        }
    }
}