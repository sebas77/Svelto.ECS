﻿using System;
 using Svelto.ECS.Internal;

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
                _weakReference.Target.CheckRemoveEntityID(new EGID(entityID, groupID), 
                                                          EntityDescriptorTemplate<T>.descriptor);
                
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityID, entityID, groupID, -1, 
                                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            public void RemoveEntity<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) where T : 
                IEntityDescriptor, new()
            {
                _weakReference.Target.CheckRemoveEntityID(new EGID(entityID, (int)groupID), 
                                                          EntityDescriptorTemplate<T>.descriptor);
                
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityID, entityID, groupID, -1, 
                                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.CheckRemoveEntityID(entityEGID, EntityDescriptorTemplate<T>.descriptor);

                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID.entityID, entityEGID.entityID, 
                                              entityEGID.groupID, 
                                              -1, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, -1, groupID, -1, null, null));
            }

            public void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, -1, groupID, -1, null, null));
            }

            public void SwapEntityGroup<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct fromGroupID, 
                                           ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, entityID, entityID, fromGroupID, toGroupID,
                                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            public void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct toGroupID) 
                where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                                              id.entityID, id.entityID, id.groupID, toGroupID,
                                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }
            
            public void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct toGroupID 
                                         , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new()
            {
                if (id.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");
                
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                        id.entityID, id.entityID, id.groupID, toGroupID,
                        EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }
            
            public void SwapEntityGroup<T>(EGID id, EGID toID) 
                where T : IEntityDescriptor, new()
            {
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                     new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                            id.entityID, toID.entityID, id.groupID, toID.groupID,
                            EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }
            
            public void SwapEntityGroup<T>(EGID id, EGID toID
                                         , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new()
            {
                if (id.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");
                
                _weakReference.Target.QueueEntitySubmitOperation<T>(
                      new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                              id.entityID, toID.entityID, id.groupID, toID.groupID,
                              EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }
        }
        
        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
#if DEBUG && !PROFILER          
            entitySubmitOperation.trace = Environment.StackTrace;
#endif            
            _entitiesOperations.AddRef(ref entitySubmitOperation);
        }

        void QueueEntitySubmitOperation<T>(EntitySubmitOperation entitySubmitOperation) where T:IEntityDescriptor
        {
#if DEBUG && !PROFILER          
            entitySubmitOperation.trace = Environment.StackTrace;
            var egid = new EGID(entitySubmitOperation.ID, entitySubmitOperation.fromGroupID);
            if (_entitiesOperationsDebug.ContainsKey(egid) == true)
                Utilities.Console.LogError("Only one entity operation per submission is allowed. Entity "
                                          .FastConcat(" with not found ID is about to be removed: ")
                                          .FastConcat(" id: ")
                                          .FastConcat(entitySubmitOperation.ID)
                                          .FastConcat(" groupid: ")
                                          .FastConcat(entitySubmitOperation.fromGroupID)
                                           //.FastConcat(entitySubmitOperation.fromGroupID.GetType().Name)); do this later
                                          .FastConcat(" entityType: ")
                                          .FastConcat(typeof(T).Name)
                                          .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                                                      " previous type: ",  _entitiesOperationsDebug[egid].ToString()));
            else
                _entitiesOperationsDebug[egid] = entitySubmitOperation.type;
#endif            
            _entitiesOperations.AddRef(ref entitySubmitOperation);
        }
#if DEBUG && !PROFILER        
        readonly Svelto.DataStructures.Experimental.FasterDictionary<long, EntitySubmitOperationType> _entitiesOperationsDebug;
#endif        
    }
}