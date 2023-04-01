using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void InitFilters()
        {
            _transientEntityFilters  = new SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection>(0);
            _persistentEntityFilters = new SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection>(0);
            _indicesOfPersistentFiltersUsedByThisComponent =
                new SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>>(0);
            _indicesOfTransientFiltersUsedByThisComponent =
                    new SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>>(0);
        }

        void DisposeFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Dispose();
            }
            
            foreach (var filter in _indicesOfTransientFiltersUsedByThisComponent)
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
            _indicesOfTransientFiltersUsedByThisComponent.Dispose();
        }

        void ClearTransientFilters()
        {
            foreach (var filter in _transientEntityFilters)
            {
                filter.value.Clear();
            }
        }

        /// <summary>
        /// Persistent filters are automatically updated by the framework. If entities are removed from the database
        /// the filters are updated consequentially.
        /// </summary>
        /// <param name="entityIDsRemoved"></param>
        /// <param name="fromGroup"></param>
        /// <param name="refWrapperType"></param>
        /// <param name="fromDic"></param>
        /// <param name="entityIDsLeftAndAffectedByRemoval"></param>
        void RemoveEntitiesFromPersistentFilters
        (FasterList<(uint entityID, string)> entityIDsRemoved, ExclusiveGroupStruct fromGroup, ComponentID refWrapperType
       , ITypeSafeDictionary fromDic, FasterList<uint> entityIDsLeftAndAffectedByRemoval)
        {
            //is there any filter used by this component?
            if (_indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(
                    refWrapperType, out NativeDynamicArrayCast<int> listOfFilters) == true)
            {
                var numberOfFilters = listOfFilters.count;
                var filters         = _persistentEntityFilters.unsafeValues;
                
                //remove duplicates
                _transientEntityIDsLeftWithoutDuplicates.Clear();
                var entityAffectedCount = entityIDsLeftAndAffectedByRemoval.count;
                for (int i = 0; i < entityAffectedCount; i++)
                {
                    _transientEntityIDsLeftWithoutDuplicates[entityIDsLeftAndAffectedByRemoval[i]] = -1;
                }

                for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
                {
                    //foreach filter linked to this component 
                    var persistentFiltersPerGroup = filters[listOfFilters[filterIndex]]._filtersPerGroup;

                    //get the filter linked to this group
                    if (persistentFiltersPerGroup.TryGetValue(fromGroup, out var fromGroupFilter))
                    {
                        var entitiesCount = entityIDsRemoved.count;

                        //foreach entity to remove, remove it from the filter (if present)
                        for (int entityIndex = 0; entityIndex < entitiesCount; ++entityIndex)
                        {
                            //the current entity id to remove
                            uint fromEntityID = entityIDsRemoved[entityIndex].entityID;

                            fromGroupFilter.Remove(fromEntityID); //Remove works even if the ID is not found (just returns false)
                        }

                        //when a component is removed from a component array, a remove swap back happens. This means
                        //that not only we have to remove the index of the component of the entity deleted from the array
                        //but we need also to update the index of the component that has been swapped in the cell
                        //of the deleted component 
                        //entityIDsAffectedByRemoval tracks all the entitiesID of the components that need to be updated
                        //in the filters because their indices in the array changed.
                        foreach (var entity in _transientEntityIDsLeftWithoutDuplicates)
                        {
                            var entityId = entity.key;
                            if (fromGroupFilter.Exists(entityId)) //does the entityID that has been swapped exist in the filter?
                            {
                                if (entity.value == -1)
                                    entity.value = (int)fromDic.GetIndex(entityId); //let's find the index of the entityID in the dictionary only once
                                
                                fromGroupFilter._entityIDToDenseIndex[entityId] = (uint) entity.value; //update the index in the filter of the component that has been swapped 
                            }
                        }
                    }
                }
            }
        }

        //this method is called by the framework only if listOfFilters.count > 0
        void SwapEntityBetweenPersistentFilters
        (FasterList<(uint, uint, string)> fromEntityToEntityIDs, ITypeSafeDictionary fromDic
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , ComponentID refWrapperType, FasterList<uint> entityIDsLeftAndAffectedByRemoval)
        {
            //is there any filter used by this component?
            if (_indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(
                    refWrapperType, out NativeDynamicArrayCast<int> listOfFilters) == true)
            {
                DBC.ECS.Check.Require(listOfFilters.count > 0, "why are you calling this with an empty list?");
                var numberOfFilters = listOfFilters.count;
                
                //remove duplicates
                _transientEntityIDsLeftWithoutDuplicates.Clear();
                var entityAffectedCount = entityIDsLeftAndAffectedByRemoval.count;
                for (int i = 0; i < entityAffectedCount; i++)
                {
                    _transientEntityIDsLeftWithoutDuplicates[entityIDsLeftAndAffectedByRemoval[i]] = -1;
                }

                /// fromEntityToEntityIDs are the IDs of the entities to swap from the from group to the to group.
                /// for this component type. for each component type, there is only one set of fromEntityToEntityIDs
                /// per from/to group.
                for (int filterIndex = 0; filterIndex < numberOfFilters; ++filterIndex)
                {
                    //if the group has a filter linked:
                    EntityFilterCollection persistentFilter =
                        _persistentEntityFilters.unsafeValues[listOfFilters[filterIndex]];
                    
                    if (persistentFilter._filtersPerGroup.TryGetValue(fromGroup, out var fromGroupFilter))
                    {
                        EntityFilterCollection.GroupFilters groupFilterTo = default;

                        foreach (var (fromEntityID, toEntityID, _) in fromEntityToEntityIDs)
                        {
                            var toIndex = toDic.GetIndex(toEntityID); //todo: optimize this should be calculated only once and not once per filter
                            //if there is an entity, it must be moved to the to filter
                            if (fromGroupFilter.Exists(fromEntityID) == true)
                            {
                                if (groupFilterTo.isValid == false)
                                    groupFilterTo = persistentFilter.GetOrCreateGroupFilter(toGroup);

                                groupFilterTo.Add(toEntityID, toIndex);
                            }
                        }

                        foreach (var (fromEntityID, _, _) in fromEntityToEntityIDs)
                        {
                            fromGroupFilter.Remove(fromEntityID); //Remove works even if the ID is not found (just returns false)
                        }

                        foreach (var entity in _transientEntityIDsLeftWithoutDuplicates)
                        {
                            var entityId = entity.key;
                            if (fromGroupFilter.Exists(entityId))
                            {
                                if (entity.value == -1)
                                    entity.value = (int)fromDic.GetIndex(entityId);
                                
                                fromGroupFilter._entityIDToDenseIndex[entityId] = (uint) entity.value;
                            }
                        }
                    }
                }
            }
        }

        internal SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _transientEntityFilters;
        internal SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _persistentEntityFilters;

        internal SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>> _indicesOfPersistentFiltersUsedByThisComponent;
        public SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>> _indicesOfTransientFiltersUsedByThisComponent;
    }
}