namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(uint entityID, BuildGroup groupID) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid) where T : IEntityDescriptor, new();
        
        void RemoveEntitiesFromGroup(BuildGroup groupID);

        void SwapEntitiesInGroup<T>(BuildGroup fromGroupID, BuildGroup toGroupID)  where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(uint entityID, BuildGroup fromGroupID, BuildGroup toGroupID)
            where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, BuildGroup toGroupID) where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, BuildGroup toGroupID, BuildGroup mustBeFromGroup)
            where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, EGID toId) where T : IEntityDescriptor, new();

        void SwapEntityGroup<T>(EGID fromID, EGID toId, BuildGroup mustBeFromGroup)
            where T : IEntityDescriptor, new();
#if UNITY_NATIVE
        NativeEntityRemove ToNativeRemove<T>(string memberName)  where T : IEntityDescriptor, new();
        NativeEntitySwap ToNativeSwap<T>(string memberName)  where T : IEntityDescriptor, new();
#endif        
    }
}