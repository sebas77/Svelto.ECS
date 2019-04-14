using System;
using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        sealed class GenericEntityFunctions : IEntityFunctions
        {
            readonly DataStructures.WeakReference<EnginesRoot> _weakReference;

            internal GenericEntityFunctions(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakReference = weakReference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(uint entityID, uint groupID) where T : IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) where T : 
                IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.CheckRemoveEntityID(entityEGID, EntityDescriptorTemplate<T>.descriptor);

                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID, entityEGID, 
                                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            
            public void RemoveEntities<T>(uint groupID) where T : IEntityDescriptor, new()
            {
                throw new NotImplementedException();
            }

            public void RemoveEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupID)
                where T : IEntityDescriptor, new()
            {
                throw new NotImplementedException();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveGroupAndEntities(uint groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, new EGID(), new EGID(0, groupID)));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID)
            {
                RemoveGroupAndEntities((uint)groupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct fromGroupID, 
                                           ExclusiveGroup.ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID) 
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(fromID, new EGID(fromID.entityID, (uint)toGroupID));
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID 
                                         , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(fromID, toGroupID);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID) 
                where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                     new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                            fromID, toID, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID
                                         , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");
                
                SwapEntityGroup<T>(fromID, toID);
            }
        }
        
        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
#if DEBUG && !PROFILER          
            entitySubmitOperation.trace = Environment.StackTrace;
#endif            
            _entitiesOperations.Add((ulong)entitySubmitOperation.fromID, ref entitySubmitOperation);
        }

        void QueueEntitySubmitOperation<T>(EntitySubmitOperation entitySubmitOperation) where T:IEntityDescriptor
        {
#if DEBUG && !PROFILER            
            entitySubmitOperation.trace = Environment.StackTrace;
            
            if (_entitiesOperations.TryGetValue((ulong) entitySubmitOperation.fromID, out var entitySubmitedOperation) == true)
            {
                if (entitySubmitedOperation != entitySubmitOperation)
                Console.LogError("Only one entity operation per submission is allowed".FastConcat(" entityViewType: ")
                                    .FastConcat(typeof(T).Name)
                                    .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                                                " from ID: ",  entitySubmitOperation.fromID.entityID.ToString())
                                    .FastConcat(            " previous operation type: ",
                                                _entitiesOperations[(ulong) entitySubmitOperation.fromID].type
                                                   .ToString()));
            }
            else
#endif            
            _entitiesOperations.Set((ulong)entitySubmitOperation.fromID, ref entitySubmitOperation);
        }
    }
}