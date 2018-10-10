using System.Diagnostics;

namespace Svelto.ECS
{
    struct EntitySubmitOperation
    {
        public readonly EntitySubmitOperationType type;
        public readonly IEntityBuilder[] builders;
        public readonly int id;
        public readonly int toGroupID;
        public readonly int fromGroupID;
#if DEBUG        
        public string trace;
#endif        

        public EntitySubmitOperation(EntitySubmitOperationType operation, int entityId, int fromGroupId, int toGroupId, IEntityBuilder[] builders)
        {
            type = operation;
            this.builders = builders;
            id = entityId;
            toGroupID = toGroupId;
            fromGroupID = fromGroupId;
#if DEBUG                    
            trace = string.Empty;
#endif            
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup
    }
}