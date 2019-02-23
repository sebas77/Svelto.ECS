using System;

namespace Svelto.ECS
{
    struct EntitySubmitOperation
    {
        public readonly EntitySubmitOperationType type;
        public readonly IEntityBuilder[]          builders;
        public readonly int                       ID;
        public readonly int                       toID;
        public readonly int                       toGroupID;
        public readonly int                       fromGroupID;
        public readonly Type                      entityDescriptor;
#if DEBUG && !PROFILER
        public string trace;
#endif

        public EntitySubmitOperation(
            EntitySubmitOperationType operation, int entityId, int toId, int fromGroupId, int toGroupId,
            IEntityBuilder[] builders, Type entityDescriptor)
        {
            type = operation;
            this.builders = builders;
            ID = entityId;
            toID = toId;

            toGroupID = toGroupId;
            fromGroupID = fromGroupId;
            this.entityDescriptor = entityDescriptor;
#if DEBUG && !PROFILER
            trace = string.Empty;
#endif
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup,
    }
}