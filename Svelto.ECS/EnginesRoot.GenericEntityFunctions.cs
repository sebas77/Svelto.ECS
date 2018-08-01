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

            public void RemoveEntity<T>(int entityID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.MoveEntity<T>(new EGID(entityID));
            }
            
            public void RemoveEntity<T>(int entityID, int groupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.MoveEntity<T>(new EGID(entityID, groupID));
            }

            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.MoveEntity<T>(entityEGID);
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.RemoveGroupAndEntitiesFromDB(groupID);
            }

            public EGID SwapEntityGroup<T>(int entityID, int fromGroupID, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(entityID, fromGroupID, toGroupID);
            }

            public EGID SwapEntityGroup<T>(EGID id, int toGroupID = ExclusiveGroup.StandardEntitiesGroup) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(id.entityID, id.groupID, toGroupID);
            }

            public EGID SwapEntityGroup<T>(int entityID, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(entityID, ExclusiveGroup.StandardEntitiesGroup, toGroupID);
            }

            public EGID SwapFirstEntityGroup<T>(int fromGroupID, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapFirstEntityInGroup<T>( fromGroupID, toGroupID);
            }
        }
    }
}