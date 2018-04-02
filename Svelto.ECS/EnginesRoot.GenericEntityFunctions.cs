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

            public GenericEntityFunctions(DataStructures.WeakReference<EnginesRoot> weakReference)
            {
                _weakReference = weakReference;
            }

            public void RemoveEntity(int entityID)
            {
                _weakReference.Target.RemoveEntity(entityID, ExclusiveGroups.StandardEntity);
            }
            
            public void RemoveEntity(int entityID, int groupID)
            {
                _weakReference.Target.RemoveEntity(entityID, groupID);
            }

            public void RemoveEntity(EGID entityEGID)
            {
                _weakReference.Target.RemoveEntity(entityEGID);
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.RemoveGroupAndEntitiesFromDB(groupID);
            }

            public void SwapEntityGroup(int entityID, int fromGroupID, int toGroupID)
            {
                _weakReference.Target.SwapEntityGroup(entityID, fromGroupID, toGroupID);
            }

            public void SwapEntityGroup(int entityID, int toGroupID)
            {
                _weakReference.Target.SwapEntityGroup(entityID, ExclusiveGroups.StandardEntity, toGroupID);
            }
        }
    }
}