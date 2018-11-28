using System;

namespace Svelto.ECS
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(int entityGidEntityId, int entityGidGroupId, Type type)
            : base("entity not found ".FastConcat(type.ToString()))
        {}
    }
}