using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public interface IEntityFunctions
    {
        /// <summary>
        /// Remove an entity from the database. Since Svelto.ECS 3.5 Removal operation behaves like this:
        /// * Remove supersedes a previous Remove operation on the same submission frame
        /// * Remove supersedes a previous Swap operation on the same submission frame if the egid is used a origin (fromEGID) - similar to the remove case
        /// * Remove throws an exception if a Build operation with the same egid is done on the same submission frame
        /// * Remove throws an exception if called on an EGID used as destination (toEGID) for a swap - similare to the build case 
        /// </summary>
        void RemoveEntity<T>(uint entityID, ExclusiveBuildGroup groupID, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void RemoveEntity<T>(EGID entityegid, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();

        /// <summary>
        /// Swap an entity between groups (subset of entities). Only one structural operation per submission frame is allowed.
        /// </summary>
        void SwapEntityGroup<T>(uint entityID, ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID,
            [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, EGID toEGID, [CallerMemberName] string caller = null) where T : IEntityDescriptor, new();
        void SwapEntityGroup<T>(EGID fromEGID, EGID toEGID, ExclusiveBuildGroup mustBeFromGroup, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new();
        
        void RemoveEntitiesFromGroup(ExclusiveBuildGroup groupID, [CallerMemberName] string caller = null);
        void SwapEntitiesInGroup(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null);

#if UNITY_NATIVE
        Svelto.ECS.Native.NativeEntityRemove ToNativeRemove<T>(string memberName) where T : IEntityDescriptor, new();
        Svelto.ECS.Native.NativeEntitySwap ToNativeSwap<T>(string memberName) where T : IEntityDescriptor, new();
#endif
    }
}