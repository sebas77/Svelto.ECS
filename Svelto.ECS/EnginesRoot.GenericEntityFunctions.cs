using System;
using Svelto.ECS.Internal;

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
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            public void RemoveEntity<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) where T : 
                IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.CheckRemoveEntityID(entityEGID, EntityDescriptorTemplate<T>.descriptor);

                _weakReference.Target.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID.entityID, entityEGID.entityID, 
                                              entityEGID.groupID, 
                                              -1, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, typeof(T)));
            }

            public void RemoveEntities<T>(int groupID) where T : IEntityDescriptor, new()
            {
                throw new NotImplementedException();
                //_weakReference.Target.QueueEntitySubmitOperation(
//                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, -1, groupID, -1, null, typeof(T)));
            }

            public void RemoveEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupID)
                where T : IEntityDescriptor, new()
            {
                throw new NotImplementedException();
                //_weakReference.Target.QueueEntitySubmitOperation(
                  //  new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, -1, groupID, -1, null, typeof(T)));
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, -1, -1, groupID, -1, null, null));
            }

            public void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID)
            {
                RemoveGroupAndEntities((int)groupID);
            }

            public void SwapEntityGroup<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct fromGroupID, 
                                           ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID);
            }

            public void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct toGroupID) 
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(id, new EGID(id.entityID, toGroupID));
            }
            
            public void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct toGroupID 
                                         , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new()
            {
                if (id.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(id, toGroupID);
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
                
                SwapEntityGroup<T>(id, toID);
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
            if (_entitiesOperationsDebug.ContainsKey((long)egid) == true)
                Console.LogError("Only one entity operation per submission is allowed. id: "
                                          .FastConcat(entitySubmitOperation.ID)
                                          .FastConcat(" groupid: ")
                                          .FastConcat(entitySubmitOperation.fromGroupID)
                                          .FastConcat(" entityType: ")
                                          .FastConcat(typeof(T).Name)
                                          .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                                                      " previous type: ",  _entitiesOperationsDebug[(long)egid].ToString()));
            else
                _entitiesOperationsDebug[(long)egid] = entitySubmitOperation.type;
#endif            
            _entitiesOperations.AddRef(ref entitySubmitOperation);
        }
#if DEBUG && !PROFILER        
        readonly Svelto.DataStructures.Experimental.FasterDictionary<long, EntitySubmitOperationType> _entitiesOperationsDebug;
#endif        
    }
}