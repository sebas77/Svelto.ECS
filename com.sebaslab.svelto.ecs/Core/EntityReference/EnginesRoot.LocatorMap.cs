using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Reference;

namespace Svelto.ECS
{
    // The EntityLocatorMap provides a bidirectional map to help locate entities without using an EGID which might
    // change in runtime. The Entity Locator map uses a reusable unique identifier struct called EntityLocator to
    // find the last known EGID from last entity submission.
    public partial class EnginesRoot
    {
        public struct EntityReferenceMap
        {
            internal EntityReference ClaimReference()
            {
                int  tempFreeIndex;
                int  newFreeIndex;
                uint version;

                do
                {
                    tempFreeIndex = _nextFreeIndex;
                    // Check if we need to create a new EntityLocator or whether we can recycle an existing one.
                    if ((uint)tempFreeIndex >= _entityReferenceMap.count)
                    {
                        newFreeIndex = tempFreeIndex + 1;
                        version      = 0;
                    }
                    else
                    {
                        ref EntityReferenceMapElement element = ref _entityReferenceMap[tempFreeIndex];
                        // The recycle entities form a linked list, using the egid.entityID to store the next element.
                        newFreeIndex = (int)element.egid.entityID;
                        version      = element.version;
                    }
                } while (tempFreeIndex != _nextFreeIndex.CompareExchange(newFreeIndex, tempFreeIndex));

#if DEBUG && !PROFILE_SVELTO
                // This code should be safe since we own the tempFreeIndex, this allows us to later check that nothing went wrong.
                if (tempFreeIndex < _entityReferenceMap.count)
                {
                    _entityReferenceMap[tempFreeIndex] = new EntityReferenceMapElement(new EGID(0, 0), version);
                }
#endif

                return new EntityReference((uint)tempFreeIndex + 1, version);
            }

            internal void SetReference(EntityReference reference, EGID egid)
            {
                // Since references can be claimed in parallel now, it might happen that they are set out of order,
                // so we need to resize instead of add.
                if (reference.index >= _entityReferenceMap.count)
                {
#if DEBUG && !PROFILE_SVELTO //THIS IS TO VALIDATE DATE DBC LIKE
                    for (var i = _entityReferenceMap.count; i <= reference.index; i++)
                    {
                        _entityReferenceMap.Add(new EntityReferenceMapElement(default, 0));
                    }
#else
                    _entityReferenceMap.AddAt(reference.index);
#endif
                }

#if DEBUG && !PROFILE_SVELTO
                // These debug tests should be enough to detect if indices are being used correctly under native factories
                if (_entityReferenceMap[reference.index].version != reference.version ||
                    _entityReferenceMap[reference.index].egid.groupID != ExclusiveGroupStruct.Invalid)
                {
                    throw new ECSException("Entity reference already set. This should never happen, please report it.");
                }
#endif

                _entityReferenceMap[reference.index] = new EntityReferenceMapElement(egid, reference.version);

                // Update reverse map from egid to locator.
                var groupMap =
                    _egidToReferenceMap.GetOrAdd(egid.groupID
                                                  , () => new SharedSveltoDictionaryNative<uint, EntityReference>(0));
                groupMap[egid.entityID] = reference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void UpdateEntityReference(EGID from, EGID to)
            {
                var reference = FetchAndRemoveReference(@from);

                _entityReferenceMap[reference.index].egid = to;

                var groupMap =
                    _egidToReferenceMap.GetOrAdd(
                        to.groupID, () => new SharedSveltoDictionaryNative<uint, EntityReference>(0));
                groupMap[to.entityID] = reference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void RemoveEntityReference(EGID egid)
            {
                var reference = FetchAndRemoveReference(@egid);

                // Invalidate the entity locator element by bumping its version and setting the egid to point to a not existing element.
                ref var entityReferenceMapElement = ref _entityReferenceMap[reference.index];
                entityReferenceMapElement.egid = new EGID((uint)(int)_nextFreeIndex, 0);
                entityReferenceMapElement.version++;

                // Mark the element as the last element used.
                _nextFreeIndex.Set((int)reference.index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            EntityReference FetchAndRemoveReference(EGID @from)
            {
                var egidToReference = _egidToReferenceMap[@from.groupID];
                var reference       = egidToReference[@from.entityID];
                egidToReference.Remove(@from.entityID);

                return reference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void RemoveAllGroupReferenceLocators(ExclusiveGroupStruct groupId)
            {
                if (_egidToReferenceMap.TryGetValue(groupId, out var groupMap) == false)
                    return;

                // We need to traverse all entities in the group and remove the locator using the egid.
                // RemoveLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
                foreach (var item in groupMap)
                    RemoveEntityReference(new EGID(item.key, groupId));

                _egidToReferenceMap.Remove(groupId);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void UpdateAllGroupReferenceLocators(ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId)
            {
                if (_egidToReferenceMap.TryGetValue(fromGroupId, out var groupMap) == false)
                    return;

                // We need to traverse all entities in the group and update the locator using the egid.
                // UpdateLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
                foreach (var item in groupMap)
                    UpdateEntityReference(new EGID(item.key, fromGroupId), new EGID(item.key, toGroupId));

                _egidToReferenceMap.Remove(fromGroupId);
            }

            public EntityReference GetEntityReference(EGID egid)
            {
                if (_egidToReferenceMap.TryGetValue(egid.groupID, out var groupMap))
                {
                    if (groupMap.TryGetValue(egid.entityID, out var locator))
                        return locator;
#if DEBUG && !PROFILE_SVELTO
                    else throw new ECSException($"Entity {egid} does not exist. Are you creating it? Try getting it from initializer.reference.");
#endif
                }

                return EntityReference.Invalid;
            }

            public bool TryGetEGID(EntityReference reference, out EGID egid)
            {
                egid = default;
                if (reference == EntityReference.Invalid)
                    return false;
                // Make sure we are querying for the current version of the locator.
                // Otherwise the locator is pointing to a removed entity.
                if (_entityReferenceMap[reference.index].version == reference.version)
                {
                    egid = _entityReferenceMap[reference.index].egid;
                    return true;
                }

                return false;
            }

            public EGID GetEGID(EntityReference reference)
            {
                if (reference == EntityReference.Invalid)
                    throw new ECSException("Invalid Reference");
                // Make sure we are querying for the current version of the locator.
                // Otherwise the locator is pointing to a removed entity.
                if (_entityReferenceMap[reference.index].version != reference.version)
                    throw new ECSException("outdated Reference");

                return _entityReferenceMap[reference.index].egid;
            }

            internal void PreallocateReferenceMaps(ExclusiveGroupStruct groupID, uint size)
            {
                _egidToReferenceMap
                   .GetOrAdd(groupID, () => new SharedSveltoDictionaryNative<uint, EntityReference>(size))
                   .EnsureCapacity(size);

                _entityReferenceMap.Resize(size);
            }

            internal void InitEntityReferenceMap()
            {
                _nextFreeIndex = SharedNativeInt.Create(0, Allocator.Persistent);
                _entityReferenceMap =
                    new NativeDynamicArrayCast<EntityReferenceMapElement>(
                        NativeDynamicArray.Alloc<EntityReferenceMapElement>());
                _egidToReferenceMap =
                    new SharedSveltoDictionaryNative<ExclusiveGroupStruct,
                        SharedSveltoDictionaryNative<uint, EntityReference>>(0);
            }

            internal void DisposeEntityReferenceMap()
            {
                _nextFreeIndex.Dispose();
                _entityReferenceMap.Dispose();

                foreach (var element in _egidToReferenceMap)
                    element.value.Dispose();
                _egidToReferenceMap.Dispose();
            }

            SharedNativeInt                                   _nextFreeIndex;
            NativeDynamicArrayCast<EntityReferenceMapElement> _entityReferenceMap;

            SharedSveltoDictionaryNative<ExclusiveGroupStruct, SharedSveltoDictionaryNative<uint, EntityReference>>
                _egidToReferenceMap;
        }

        EntityReferenceMap entityLocator => _entityLocator;
        
        EntityReferenceMap          _entityLocator;
    }
}