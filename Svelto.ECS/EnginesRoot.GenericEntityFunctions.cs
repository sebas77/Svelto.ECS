﻿using Svelto.ECS.Internal;

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
                _weakReference.Target.MoveEntity<T>(new EGID(entityID, groupID));
            }

            public void RemoveEntity<T>(int entityID, ExclusiveGroup groupID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.MoveEntity<T>(new EGID(entityID, (int) groupID));
            }

            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.Target.MoveEntity<T>(entityEGID);
            }

            public void RemoveGroupAndEntities(int groupID)
            {
                _weakReference.Target.RemoveGroupAndEntitiesFromDB(groupID);
            }

            public void RemoveGroupAndEntities(ExclusiveGroup groupID)
            {
                _weakReference.Target.RemoveGroupAndEntitiesFromDB((int) groupID);
            }

            public EGID SwapEntityGroup<T>(int entityID, int fromGroupID, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(entityID, fromGroupID, toGroupID);
            }

            public EGID SwapEntityGroup<T>(int entityID, ExclusiveGroup fromGroupID, ExclusiveGroup toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(entityID, (int) fromGroupID, (int) toGroupID);
            }

            public EGID SwapEntityGroup<T>(EGID id, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(id.entityID, id.groupID, toGroupID);
            }

            public EGID SwapEntityGroup<T>(EGID id, ExclusiveGroup toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapEntityGroup<T>(id.entityID, id.groupID, (int) toGroupID);
            }

            public EGID SwapFirstEntityGroup<T>(int fromGroupID, int toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapFirstEntityInGroup<T>( fromGroupID, toGroupID);
            }

            public EGID SwapFirstEntityGroup<T>(ExclusiveGroup fromGroupID, ExclusiveGroup toGroupID) where T : IEntityDescriptor, new()
            {
                return _weakReference.Target.SwapFirstEntityInGroup<T>( (int) fromGroupID, (int) toGroupID);
            }
        }
    }
}