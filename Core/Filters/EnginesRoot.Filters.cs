using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void InitFilters()
        {
            _transientEntityFilters  = new SharedSveltoDictionaryNative<long, EntityFilterCollection>(0);
            _persistentEntityFilters = new SharedSveltoDictionaryNative<long, EntityFilterCollection>(0);
            _indicesOfPersistentFiltersUsedByThisComponent =
                new SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>(0);
        }

        void DisposeFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Dispose();
            }

            foreach (var filter in _persistentEntityFilters)
            {
                filter.value.Dispose();
            }

            foreach (var filter in _indicesOfPersistentFiltersUsedByThisComponent)
            {
                filter.value.Dispose();
            }

            _transientEntityFilters.Dispose();
            _persistentEntityFilters.Dispose();
            _indicesOfPersistentFiltersUsedByThisComponent.Dispose();
        }

        void ClearTransientFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Clear();
            }
        }

        void RemoveEntityFromPersistentFilters(FasterList<(uint, string)> entityIDs, ExclusiveGroupStruct fromGroup,
            RefWrapperType refWrapperType, ITypeSafeDictionary fromDic)
        {
            //is there any filter used by this component?
            if (_indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(new NativeRefWrapperType(refWrapperType),
                    out NativeDynamicArrayCast<int> listOfFilters))
            {
                var numberOfFilters = listOfFilters.count;
                for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
                {
                    //we are going to remove multiple entities, this means that the dictionary count would decrease 
                    //for each entity removed from each filter
                    //we need to keep a copy to reset to the original count for each filter
                    var currentLastIndex = (uint)fromDic.count - 1;
                    var filters          = _persistentEntityFilters.unsafeValues;
                    var persistentFilter = filters[listOfFilters[filterIndex]]._filtersPerGroup;
                    
                    if (persistentFilter.TryGetValue(fromGroup, out var groupFilter))
                    {
                        var entitiesCount = entityIDs.count;
                        
                        for (int entityIndex = 0; entityIndex < entitiesCount; ++entityIndex)
                        {
                            uint fromentityID = entityIDs[entityIndex].Item1;
                            var  fromIndex    = fromDic.GetIndex(fromentityID);

                            groupFilter.RemoveWithSwapBack(fromentityID, fromIndex, currentLastIndex--);
                        }
                    }
                }
            }
        }

        //this method is called by the framework only if listOfFilters.count > 0
        void SwapEntityBetweenPersistentFilters(FasterList<(uint, uint, string)> fromEntityToEntityIDs,
            FasterDictionary<uint, uint> beforeSubmissionFromIDs, ITypeSafeDictionary toComponentsDictionary,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, uint fromDictionaryCount,
            NativeDynamicArrayCast<int> listOfFilters)
        {
            DBC.ECS.Check.Require(listOfFilters.count > 0, "why are you calling this with an empty list?");
            var numberOfFilters = listOfFilters.count;

            /// fromEntityToEntityIDs are the ID of the entities to swap from the from group to the to group.
            /// for this component type. for each component type, there is only one set of fromEntityToEntityIDs
            /// per from/to group.
            /// The complexity of this function is that the ToDictionary is already updated, so the toIndex
            /// is actually correct and guaranteed to be valid. However the beforeSubmissionFromIDs are the
            /// indices of the entities in the FromDictionary BEFORE the submission happens, so before the
            /// entities are actually removed from the dictionary.
            for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
            {
                //we are going to remove multiple entities, this means that the dictionary count would decrease 
                //for each entity removed from each filter
                //we need to keep a copy to reset to the original count for each filter
                var currentLastIndex = fromDictionaryCount;

                //if the group has a filter linked:
                EntityFilterCollection persistentFilter =
                    _persistentEntityFilters.unsafeValues[listOfFilters[filterIndex]];
                if (persistentFilter._filtersPerGroup.TryGetValue(fromGroup, out var fromGroupFilter))
                {
                    EntityFilterCollection.GroupFilters groupFilterTo = default;

                    foreach (var (fromEntityID, toEntityID, _) in fromEntityToEntityIDs)
                    {
                        //if there is an entity, it must be moved to the to filter
                        if (fromGroupFilter.Exists(fromEntityID) == true)
                        {
                            var toIndex = toComponentsDictionary.GetIndex(toEntityID);

                            if (groupFilterTo.isValid == false)
                                groupFilterTo = persistentFilter.GetGroupFilter(toGroup);

                            groupFilterTo.Add(toEntityID, toIndex);
                        }
                    }

                    foreach (var (fromEntityID, _, _) in fromEntityToEntityIDs)
                    {
                        //fromIndex is the same of the index in the filter if the entity is in the filter, but
                        //we need to update the entity index of the last entity swapped from the dictionary even
                        //in the case when the fromEntity is not present in the filter.

                        uint fromIndex; //index in the from dictionary
                        if (fromGroupFilter.Exists(fromEntityID))
                            fromIndex = fromGroupFilter._entityIDToDenseIndex[fromEntityID];
                        else
                            fromIndex = beforeSubmissionFromIDs[fromEntityID];

                        //Removing an entity from the dictionary may affect the index of the last entity in the
                        //values dictionary array, so we need to to update the indices of the affected entities.
                        //must be outside because from may not be present in the filter, but last index is

                        //for each entity removed from the from group, I have to update it's index in the
                        //from filter. An entity removed from the DB is always swapped back, which means
                        //it's current position is taken by the last entity in the dictionary array.

                        //this means that the index of the last entity will change to the index of the
                        //replaced entity

                        fromGroupFilter.RemoveWithSwapBack(fromEntityID, fromIndex, currentLastIndex--);
                    }
                }
            }
        }

        internal SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;
        internal SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

        internal SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
            _indicesOfPersistentFiltersUsedByThisComponent;
    }
}