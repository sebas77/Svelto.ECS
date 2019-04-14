using System;

namespace Svelto.ECS
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(EGID entityGidEntityId, Type type)
            : base("entity not found ".FastConcat(type.ToString()))
        {}
    }
}