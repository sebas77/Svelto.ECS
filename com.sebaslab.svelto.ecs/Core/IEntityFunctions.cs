using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        //being entity ID globally not unique, the group must be specified when
        //an entity is removed. Not specifying the group will attempt to remove
        //the entity from the special standard group.
        void RemoveEntity<T>(uint entityID, ExclusiveBuildGroup groupID , [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid , [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        
        void RemoveEntitiesFromGroup(ExclusiveBuildGroup groupID , [CallerMemberName] string caller = null);
        void SwapEntitiesInGroup(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null);

        void SwapEntityGroup<T>(uint entityID, ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, EGID toEGID, [CallerMemberName] string caller = null)where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, EGID toEGID, ExclusiveBuildGroup mustBeFromGroup, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
#if UNITY_NATIVE
        Svelto.ECS.Native.NativeEntityRemove ToNativeRemove<T>(string memberName)  where T : IEntityDescriptor, new();
        Svelto.ECS.Native.NativeEntitySwap  ToNativeSwap<T>(string memberName)  where T : IEntityDescriptor, new();
#endif        
    }
}