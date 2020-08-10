using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        // The EntityLocatorMap provides a bidirectional map to help locate entities without using an EGID which might
        // change in runtime. The Entity Locator map uses a reusable unique identifier struct called EntityLocator to
        // find the last known EGID from last entity submission.
        class EntityLocatorMap : IEntityLocatorMap
        {
            public EntityLocatorMap(EnginesRoot enginesRoot)
            {
                _enginesRoot = new WeakReference<EnginesRoot>(enginesRoot);
            }

            public EntityLocator GetLocator(EGID egid)
            {
                return _enginesRoot.Target.GetLocator(egid);
            }

            public EGID GetEGID(EntityLocator locator)
            {
                return _enginesRoot.Target.FindEGID(locator);
            }

            WeakReference<EnginesRoot> _enginesRoot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void CreateLocator(EGID egid)
        {
            // Check if we need to create a new EntityLocator or whether we can recycle an existing one.s
            EntityLocator locator;
            if (_nextEntityId == _entityLocatorMap.count)
            {
                _entityLocatorMap.Add(new EntityLocatorMapElement(egid));
                locator = new EntityLocator(_nextEntityId++);
            }
            else
            {
                ref var element = ref _entityLocatorMap[_nextEntityId];
                locator = new EntityLocator(_nextEntityId, element.version);
                // The recycle entities form a linked list, using the egid.entityID to store the next element.
                _nextEntityId = element.egid.entityID;
                element.egid = egid;
            }

            // When we create a new one there is nothing to recycle anymore, so we need to update the last recycle entityId.
            if (_nextEntityId == _entityLocatorMap.count)
            {
                _lastEntityId = _entityLocatorMap.count;
            }

            // Update reverse map from egid to locator.
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap) == false)
            {
                groupMap = new FasterDictionary<uint, EntityLocator>();
                _egidToLocatorMap[egid.groupID] = groupMap;
            }
            groupMap[egid.entityID] = locator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateLocator(EGID from, EGID to)
        {
            var locator = GetAndRemoveLocator(from);

#if DEBUG && !PROFILE_SVELTO
            if (locator.Equals(EntityLocator.Invalid))
            {
                throw new ECSException("Unable to update locator from egid: "
                    .FastConcat(from.ToString(), "to egid: ")
                    .FastConcat(to.ToString(), ". Locator was not found")
                );
            }
#endif

            _entityLocatorMap[locator.uniqueID].egid = to;

            if (_egidToLocatorMap.TryGetValue(to.groupID, out var groupMap) == false)
            {
                groupMap = new FasterDictionary<uint, EntityLocator>();
                _egidToLocatorMap[to.groupID] = groupMap;
            }
            groupMap[to.entityID] = locator;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveLocator(EGID egid)
        {
            var locator = GetAndRemoveLocator(egid);

#if DEBUG && !PROFILE_SVELTO
            if (locator.Equals(EntityLocator.Invalid))
            {
                throw new ECSException("Unable to remove locator for egid: "
                    .FastConcat(egid.ToString(), ". Locator was not found")
                );
            }
#endif

            // Check if this is the first recycled element.
            if (_lastEntityId == _entityLocatorMap.count)
            {
                _nextEntityId = locator.uniqueID;
            }
            // Otherwise add it as the last recycled element.
            else
            {
                _entityLocatorMap[_lastEntityId].egid = new EGID(locator.uniqueID, 0);
            }

            // Invalidate the entity locator element by bumping its version and setting the egid to point to a unexisting element.
            _entityLocatorMap[locator.uniqueID].egid = new EGID(_entityLocatorMap.count, 0);
            _entityLocatorMap[locator.uniqueID].version++;

            // Mark the element as the last element used.
            _lastEntityId = locator.uniqueID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveAllGroupLocators(uint groupId)
        {
            if (_egidToLocatorMap.TryGetValue(groupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and remove the locator using the egid.
            // RemoveLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                RemoveLocator(new EGID(keys[i].key, groupId));
                if (i == 0) break;
            }

            _egidToLocatorMap.Remove(groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateAllGroupLocators(uint fromGroupId, uint toGroupId)
        {
            if (_egidToLocatorMap.TryGetValue(fromGroupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and update the locator using the egid.
            // UpdateLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                UpdateLocator(new EGID(keys[i].key, fromGroupId), new EGID(keys[i].key, toGroupId));
                if (i == 0) break;
            }

            _egidToLocatorMap.Remove(fromGroupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityLocator GetLocator(EGID egid)
        {
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap))
            {
                if (groupMap.TryGetValue(egid.entityID, out var locator))
                {
                    return locator;
                }
            }

            return EntityLocator.Invalid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityLocator GetAndRemoveLocator(EGID egid)
        {
            if (_egidToLocatorMap.TryGetValue(egid.groupID, out var groupMap))
            {
                if (groupMap.TryGetValue(egid.entityID, out var locator))
                {
                    groupMap.Remove(egid.entityID);
                    return locator;
                }
            }

            return EntityLocator.Invalid;
        }

        EGID FindEGID(EntityLocator locator)
        {
#if DEBUG && !PROFILE_SVELTO
            if (locator.uniqueID >= _entityLocatorMap.count)
            {
                throw new ECSException("EntityLocator is out of bounds.");
            }
#endif
            // Make sure we are querying for the current version of the locator.
            // Otherwise the locator is pointing to a removed entity.
            if (_entityLocatorMap[locator.uniqueID].version == locator.version)
            {
                return _entityLocatorMap[locator.uniqueID].egid;
            }
            else
            {
#if DEBUG && !PROFILE_SVELTO
                throw new ECSException("Attempting to find EGID with outdated entityLocator");
#endif
                return new EGID();
            }
        }

        uint _nextEntityId;
        uint _lastEntityId;
        readonly FasterList<EntityLocatorMapElement> _entityLocatorMap;
        readonly FasterDictionary<uint, FasterDictionary<uint, EntityLocator>> _egidToLocatorMap;
    }
}