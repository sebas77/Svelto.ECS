namespace Svelto.ECS
{
    struct EntitySubmitOperation
    {
        public readonly EntitySubmitOperationType type;
        public readonly IEntityBuilder[] builders;
        public readonly int id;
        public readonly int toGroupID;
        public readonly int fromGroupID;

        public EntitySubmitOperation(EntitySubmitOperationType operation, int entityId, int fromGroupId, int toGroupId, IEntityBuilder[] builders)
        {
            type = operation;
            this.builders = builders;
            id = entityId;
            toGroupID = toGroupId;
            fromGroupID = fromGroupId;
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup
    }
}