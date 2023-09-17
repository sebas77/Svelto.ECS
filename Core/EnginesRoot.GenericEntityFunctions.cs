using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class GenericEntityFunctions : IEntityFunctions
        {
            internal GenericEntityFunctions(EnginesRoot weakReference)
            {
                _enginesRoot = new WeakReference<EnginesRoot>(weakReference);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>
                (uint entityID, ExclusiveBuildGroup groupID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID), caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(EGID entityEGID, [CallerMemberName] string caller = null) 
                where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(entityEGID.groupID.isInvalid == false, "invalid group detected");
                _enginesRoot.Target.CheckRemoveEntityID(entityEGID, TypeCache<T>.type, caller);

                _enginesRoot.Target.QueueRemoveEntityOperation(
                    entityEGID, _enginesRoot.Target.FindRealComponents<T>(entityEGID), caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntitiesFromGroup(ExclusiveBuildGroup groupID, [CallerMemberName] string caller = null)
            {
                DBC.ECS.Check.Require(groupID.isInvalid == false, "invalid group detected");
                _enginesRoot.Target.RemoveGroupID(groupID);

                _enginesRoot.Target.QueueRemoveGroupOperation(groupID, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntitiesInGroup
            (ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null) 
            {
                if (_enginesRoot.Target._groupEntityComponentsDB.TryGetValue(
                        fromGroupID.group
                      , out FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType) == true)
                {
#if DEBUG && !PROFILE_SVELTO
                    ITypeSafeDictionary dictionary = entitiesInGroupPerType.unsafeValues[0];

                    dictionary.KeysEvaluator((key) =>
                    {
                        _enginesRoot.Target.CheckSwapEntityID(new EGID(key, fromGroupID), new EGID(key, toGroupID), null, caller);
                    });
#endif
                    _enginesRoot.Target.QueueSwapGroupOperation(fromGroupID, toGroupID, caller);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>
            (uint entityID, ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID
           , [CallerMemberName] string caller = null) where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>
                (EGID fromEGID, ExclusiveBuildGroup toGroupID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(fromEGID, new EGID(fromEGID.entityID, toGroupID), caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>
                (EGID fromEGID, EGID toEGID, ExclusiveBuildGroup mustBeFromGroup, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                if (fromEGID.groupID != mustBeFromGroup)
                    throw new ECSException(
                        $"Entity is not coming from the expected group Expected {mustBeFromGroup} is {fromEGID.groupID}");

                SwapEntityGroup<T>(fromEGID, toEGID, caller);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromEGID, EGID toEGID, [CallerMemberName] string caller = null)
                where T : IEntityDescriptor, new()
            {
                DBC.ECS.Check.Require(fromEGID.groupID.isInvalid == false, "invalid group detected");
                DBC.ECS.Check.Require(toEGID.groupID.isInvalid == false, "invalid group detected");

                var enginesRootTarget = _enginesRoot.Target;

                enginesRootTarget.CheckSwapEntityID(fromEGID, toEGID, TypeCache<T>.type, caller);

                enginesRootTarget.QueueSwapEntityOperation(fromEGID, toEGID
                                                         , _enginesRoot.Target.FindRealComponents<T>(fromEGID)
                                                         , caller);
            }

#if UNITY_NATIVE
            public Native.NativeEntityRemove ToNativeRemove<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntityRemoveQueue<T>(memberName);
            }

            public Native.NativeEntitySwap ToNativeSwap<T>(string memberName) where T : IEntityDescriptor, new()
            {
                return _enginesRoot.Target.ProvideNativeEntitySwapQueue<T>(memberName);
            }
#endif
            //enginesRoot is a weakreference because GenericEntityStreamConsumerFactory can be injected inside
            //engines of other enginesRoot
            readonly WeakReference<EnginesRoot> _enginesRoot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void QueueRemoveGroupOperation(ExclusiveBuildGroup groupID, string caller)
        {
            _entitiesOperations.QueueRemoveGroupOperation(groupID, caller);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void QueueSwapGroupOperation(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, string caller)
        {
            _entitiesOperations.QueueSwapGroupOperation(fromGroupID, toGroupID, caller);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void QueueSwapEntityOperation(EGID fromID, EGID toID, IComponentBuilder[] componentBuilders, string caller)
        {
            _entitiesOperations.QueueSwapOperation(fromID, toID, componentBuilders, caller);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void QueueRemoveEntityOperation(EGID entityEGID, IComponentBuilder[] componentBuilders, string caller)
        {
            _entitiesOperations.QueueRemoveOperation(entityEGID, componentBuilders, caller);
        }
    }
}