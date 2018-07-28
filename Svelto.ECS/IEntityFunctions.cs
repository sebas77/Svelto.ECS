using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specificing the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(int entityID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(int entityID, int groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();

        void RemoveGroupAndEntities(int groupID);
        
        void SwapEntityGroup<T>(int entityID, int fromGroupID, int toGroupID = ExclusiveGroup.StandardEntitiesGroup) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id, int toGroupID = ExclusiveGroup.StandardEntitiesGroup) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(int entityID, int toGroupID) where T : IEntityDescriptor, new();
        EGID SwapFirstEntityGroup<T>(int fromGroupID = ExclusiveGroup.StandardEntitiesGroup, int toGroupID = ExclusiveGroup.StandardEntitiesGroup) where T : IEntityDescriptor, new();
    }
}