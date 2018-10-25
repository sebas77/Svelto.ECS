﻿using System;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFunctions : IEntityFunctions
        {
            readonly DataStructures.WeakReference<EnginesRoot> _weakReference;

            internal GenericEntityFunctions(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakReference = weakReference;
            }

            public void RemoveEntity<T>(int entityID, int groupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityID, groupID, -1, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            public void RemoveEntity<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct  groupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityID, (int)groupID, -1, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID.entityID, entityEGID.groupID, -1, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, groupID, -1, null));
            }

            public void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct  groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, (int)groupID, -1, null));
            }

            public void SwapEntityGroup<T>(int entityID, int fromGroupID, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, entityID, fromGroupID, (int)toGroupID, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            public void SwapEntityGroup<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct  fromGroupID, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, entityID, (int) fromGroupID, (int) toGroupID, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            public void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, id.entityID, id.groupID, (int)toGroupID, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }
        }

        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
#if DEBUG            
            entitySubmitOperation.trace = Environment.StackTrace;
#endif            
            _entitiesOperations.AddRef(ref entitySubmitOperation);
        }
    }
}