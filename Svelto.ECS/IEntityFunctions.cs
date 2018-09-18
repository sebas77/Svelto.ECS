using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(int entityID, int groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(int entityID, ExclusiveGroup groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();

        void RemoveGroupAndEntities(int groupID);
        void RemoveGroupAndEntities(ExclusiveGroup groupID);
        
        void SwapEntityGroup<T>(int entityID, int fromGroupID, int toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(int entityID, ExclusiveGroup fromGroupID, ExclusiveGroup toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id, int toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id, ExclusiveGroup toGroupID) where T : IEntityDescriptor, new();
    }
}