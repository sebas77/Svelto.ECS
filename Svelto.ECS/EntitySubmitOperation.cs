using System;

namespace Svelto.ECS
{
#pragma warning disable 660,661
    struct EntitySubmitOperation
#pragma warning restore 660,661
        : IEquatable<EntitySubmitOperation>
    {
        public readonly EntitySubmitOperationType type;
        public readonly IEntityBuilder[]          builders;
        public readonly EGID                      fromID;
        public readonly EGID                      toID;
        public readonly Type                      entityDescriptor;
#if DEBUG && !PROFILER
        public string trace;
#endif

        public EntitySubmitOperation(EntitySubmitOperationType operation, EGID from, EGID to,
                                     IEntityBuilder[]          builders         = null,
                                     Type                      entityDescriptor = null)
        {
            type          = operation;
            this.builders = builders;
            fromID            = from;
            toID          = to;

            this.entityDescriptor = entityDescriptor;
#if DEBUG && !PROFILER
            trace = string.Empty;
#endif
        }
        
        public static bool operator ==(EntitySubmitOperation obj1, EntitySubmitOperation obj2)
        {
            return obj1.Equals(obj2);
        }
        
        public static bool operator !=(EntitySubmitOperation obj1, EntitySubmitOperation obj2)
        {
            return obj1.Equals(obj2) == false;
        }

        public bool Equals(EntitySubmitOperation other)
        {
            return type == other.type && fromID == other.fromID && toID == other.toID;
        }
    }

    enum EntitySubmitOperationType
    {
        Swap,
        Remove,
        RemoveGroup
    }
}