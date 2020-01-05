using System;

namespace Svelto.ECS
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(EGID entityEGID, Type entityType) : base(
            $"entity of type '{entityType}' with ID '{entityEGID.entityID}', group '{(uint) entityEGID.groupID}' not found!")
        {
        }
    }
}