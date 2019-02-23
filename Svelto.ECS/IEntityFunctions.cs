namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(int entityID, int groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct  groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();
        
        void RemoveEntities<T>(int groupID) where T : IEntityDescriptor, new();
        void RemoveEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupID)  where T : IEntityDescriptor, new();

        void RemoveGroupAndEntities(int groupID);
        void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID);
        
        void SwapEntityGroup<T>(int entityID, ExclusiveGroup.ExclusiveGroupStruct  fromGroupID, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct  toGroupID) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id, ExclusiveGroup.ExclusiveGroupStruct toGroupID, ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new();
        
        void SwapEntityGroup<T>(EGID id,       EGID toId) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID id,       EGID toId, ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup) where T : IEntityDescriptor, new();
    }
}