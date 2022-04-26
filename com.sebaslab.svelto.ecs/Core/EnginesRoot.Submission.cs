using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SingleSubmission(PlatformProfiler profiler)
        {
            ClearDebugChecks(); //this must be done first as I need the carry the last states after the submission

            _entitiesOperations.ExecuteRemoveAndSwappingOperations(_swapEntities, _removeEntities, _removeGroup,
                _swapGroup, this);

            AddEntities(profiler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void RemoveGroup(ExclusiveGroupStruct groupID, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("remove whole group"))
            {
                enginesRoot.RemoveEntitiesFromGroup(groupID, sampler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SwapGroup(ExclusiveGroupStruct fromGroupID, ExclusiveGroupStruct toGroupID, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("swap whole group"))
            {
                enginesRoot.SwapEntitiesBetweenGroups(fromGroupID, toGroupID, sampler);
            }
        }

        static void RemoveEntities(
            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>
                removeOperations, FasterList<EGID> entitiesRemoved, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("remove Entities"))
            {
                using (sampler.Sample("Remove Entity References"))
                {
                    var count = entitiesRemoved.count;
                    for (int i = 0; i < count; i++)
                    {
                        enginesRoot._entityLocator.RemoveEntityReference(entitiesRemoved[i]);
                    }
                }

                using (sampler.Sample("Execute remove callbacks and remove entities"))
                {
                    foreach (var entitiesToRemove in removeOperations)
                    {
                        ExclusiveGroupStruct group               = entitiesToRemove.key;
                        var                  fromGroupDictionary = enginesRoot.GetDBGroup(group);

                        foreach (var groupedEntitiesToRemove in entitiesToRemove.value)
                        {
                            var                 componentType            = groupedEntitiesToRemove.key;
                            ITypeSafeDictionary fromComponentsDictionary = fromGroupDictionary[componentType];

                            FasterList<(uint, string)> infosToProcess = groupedEntitiesToRemove.value;

                            fromComponentsDictionary.ExecuteEnginesRemoveCallbacks(infosToProcess,
                                enginesRoot._reactiveEnginesRemove, group, in sampler);
                        }
                    }
                }

                using (sampler.Sample("Remove Entities"))
                {
                    enginesRoot._cachedRangeOfSubmittedIndices.FastClear();

                    foreach (var entitiesToRemove in removeOperations)
                    {
                        ExclusiveGroupStruct fromGroup           = entitiesToRemove.key;
                        var                  fromGroupDictionary = enginesRoot.GetDBGroup(fromGroup);

                        foreach (var groupedEntitiesToRemove in entitiesToRemove.value)
                        {
                            RefWrapperType      componentType            = groupedEntitiesToRemove.key;
                            ITypeSafeDictionary fromComponentsDictionary = fromGroupDictionary[componentType];

                            FasterList<(uint, string)> entityIDsToRemove = groupedEntitiesToRemove.value;

                            enginesRoot.RemoveEntityFromPersistentFilters(entityIDsToRemove, fromGroup,
                                    componentType, fromComponentsDictionary);
                            
                            fromComponentsDictionary.RemoveEntitiesFromDictionary(entityIDsToRemove);
                            
                            //store new count after the entities are removed, plus the number of entities removed
                            enginesRoot._cachedRangeOfSubmittedIndices.Add(((uint, uint))(
                                fromComponentsDictionary.count,
                                fromComponentsDictionary.count + entityIDsToRemove.count));
                        }
                    }
                }
                
                var rangeEnumerator = enginesRoot._cachedRangeOfSubmittedIndices.GetEnumerator();
                using (sampler.Sample("Execute remove Callbacks Fast"))
                {
                    foreach (var entitiesToRemove in removeOperations)
                    {
                        ExclusiveGroupStruct group               = entitiesToRemove.key;
                        var                  fromGroupDictionary = enginesRoot.GetDBGroup(group);

                        foreach (var groupedEntitiesToRemove in entitiesToRemove.value)
                        {
                            rangeEnumerator.MoveNext();
                            
                            var                 componentType            = groupedEntitiesToRemove.key;
                            ITypeSafeDictionary fromComponentsDictionary = fromGroupDictionary[componentType];

                            //get all the engines linked to TValue
                            if (!enginesRoot._reactiveEnginesRemoveEx.TryGetValue(new RefWrapperType(componentType),
                                    out var entityComponentsEngines))
                                continue;
                            
                            fromComponentsDictionary.ExecuteEnginesRemoveCallbacksFast(entityComponentsEngines,
                                group, rangeEnumerator.Current, in sampler);
                        }
                    }
                }
            }
        }

        static void SwapEntities(
            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>> swapEntitiesOperations,
            FasterList<(EGID, EGID)> entitiesIDSwaps, EnginesRoot enginesRoot)
        {
            using (var sampler = new PlatformProfiler("Swap entities between groups"))
            {
                using (sampler.Sample("Update Entity References"))
                {
                    var count = entitiesIDSwaps.count;
                    for (int i = 0; i < count; i++)
                    {
                        var (fromEntityGid, toEntityGid) = entitiesIDSwaps[i];

                        enginesRoot._entityLocator.UpdateEntityReference(fromEntityGid, toEntityGid);
                    }
                }

                using (sampler.Sample("Swap Entities"))
                {
                    enginesRoot._cachedRangeOfSubmittedIndices.FastClear();
                    //Entities to swap are organised in order to minimise the amount of dictionary lookups.
                    //swapEntitiesOperations iterations happen in the following order:
                    //for each fromGroup, get all the entities to swap for each component type.
                    //then get the list of IDs for each ToGroup.
                    //now swap the set of FromGroup -> ToGroup entities per ID.
                    foreach (var entitiesToSwap in swapEntitiesOperations)
                    {
                        ExclusiveGroupStruct fromGroup           = entitiesToSwap.key;
                        var                  fromGroupDictionary = enginesRoot.GetDBGroup(fromGroup);

                        //iterate all the fromgroups
                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var                 componentType            = groupedEntitiesToSwap.key;
                            ITypeSafeDictionary fromComponentsDictionary = fromGroupDictionary[componentType];

                            //get the subset of togroups that come from from group
                            foreach (var entitiesInfoToSwap in groupedEntitiesToSwap.value)
                            {
                                ExclusiveGroupStruct toGroup = entitiesInfoToSwap.key;
                                ITypeSafeDictionary toComponentsDictionary =
                                    enginesRoot.GetOrAddTypeSafeDictionary(toGroup,
                                        enginesRoot.GetOrAddDBGroup(toGroup), componentType, fromComponentsDictionary);

                                DBC.ECS.Check.Assert(toComponentsDictionary != null,
                                    "something went wrong with the creation of dictionaries");

                                //this list represents the set of entities that come from fromGroup and need
                                //to be swapped to toGroup. Most of the times will be 1 of few units.
                                FasterList<(uint, uint, string)> fromEntityToEntityIDs = entitiesInfoToSwap.value;

                                int fromDictionaryCountBeforeSubmission = -1;

                                if (enginesRoot._indicesOfPersistentFiltersUsedByThisComponent.TryGetValue(
                                        new NativeRefWrapperType(componentType),
                                        out NativeDynamicArrayCast<int> listOfFilters))
                                {
                                    enginesRoot._cachedIndicesToSwapBeforeSubmissionForFilters.FastClear();

                                    fromDictionaryCountBeforeSubmission = fromComponentsDictionary.count - 1;

                                    //add the index of the entities in the component array for each entityID
                                    //BEFORE the submission, as after that the ID will be different
                                    foreach (var (fromEntityID, _, _) in fromEntityToEntityIDs)
                                        enginesRoot._cachedIndicesToSwapBeforeSubmissionForFilters.Add(fromEntityID,
                                            fromComponentsDictionary.GetIndex(fromEntityID));
                                }

                                //ensure that to dictionary has enough room to store the new entities`
                                toComponentsDictionary.EnsureCapacity((uint)(toComponentsDictionary.count +
                                    (uint)fromEntityToEntityIDs.count));

                                //fortunately swap means that entities are added at the end of each destination
                                //dictionary list, so we can just iterate the list using the indices ranges added in the
                                //_cachedIndices
                                enginesRoot._cachedRangeOfSubmittedIndices.Add(((uint, uint))(
                                    toComponentsDictionary.count,
                                    toComponentsDictionary.count + fromEntityToEntityIDs.count));

                                fromComponentsDictionary.SwapEntitiesBetweenDictionaries(fromEntityToEntityIDs,
                                    fromGroup, toGroup, toComponentsDictionary);

                                if (fromDictionaryCountBeforeSubmission != -1) //this if skips the swap if there are no filters linked to the component
                                    enginesRoot.SwapEntityBetweenPersistentFilters(fromEntityToEntityIDs,
                                        enginesRoot._cachedIndicesToSwapBeforeSubmissionForFilters,
                                        toComponentsDictionary, fromGroup, toGroup,
                                        (uint)fromDictionaryCountBeforeSubmission, listOfFilters);
                            }
                        }
                    }
                }

                using (sampler.Sample("Execute Swap Callbacks"))
                {
                    foreach (var entitiesToSwap in swapEntitiesOperations)
                    {
                        ExclusiveGroupStruct fromGroup = entitiesToSwap.key;

                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var componentType = groupedEntitiesToSwap.key;

                            //get all the engines linked to TValue
                            if (!enginesRoot._reactiveEnginesSwap.TryGetValue(new RefWrapperType(componentType),
                                    out var entityComponentsEngines))
                                continue;

                            foreach (var entitiesInfoToSwap in groupedEntitiesToSwap.value)
                            {
                                ExclusiveGroupStruct toGroup = entitiesInfoToSwap.key;
                                ITypeSafeDictionary toComponentsDictionary = GetTypeSafeDictionary(toGroup,
                                    enginesRoot.GetDBGroup(toGroup), componentType);

                                var infosToProcess = entitiesInfoToSwap.value;

                                toComponentsDictionary.ExecuteEnginesSwapCallbacks(infosToProcess,
                                    entityComponentsEngines, fromGroup, toGroup, in sampler);
                            }
                        }
                    }
                }

                var rangeEnumerator = enginesRoot._cachedRangeOfSubmittedIndices.GetEnumerator();
                using (sampler.Sample("Execute Swap Callbacks Fast"))
                {
                    foreach (var entitiesToSwap in swapEntitiesOperations)
                    {
                        ExclusiveGroupStruct fromGroup = entitiesToSwap.key;

                        foreach (var groupedEntitiesToSwap in entitiesToSwap.value)
                        {
                            var componentType = groupedEntitiesToSwap.key;

                            foreach (var entitiesInfoToSwap in groupedEntitiesToSwap.value)
                            {
                                rangeEnumerator.MoveNext();

                                //get all the engines linked to TValue
                                if (!enginesRoot._reactiveEnginesSwapEx.TryGetValue(new RefWrapperType(componentType),
                                        out var entityComponentsEngines))
                                    continue;

                                ExclusiveGroupStruct toGroup = entitiesInfoToSwap.key;
                                ITypeSafeDictionary toComponentsDictionary = GetTypeSafeDictionary(toGroup,
                                    enginesRoot.GetDBGroup(toGroup), componentType);

                                toComponentsDictionary.ExecuteEnginesSwapCallbacksFast(entityComponentsEngines,
                                    fromGroup, toGroup, rangeEnumerator.Current, in sampler);
                            }
                        }
                    }
                }
            }
        }

        void AddEntities(PlatformProfiler sampler)
        {
            //current buffer becomes other, and other becomes current
            _groupedEntityToAdd.Swap();

            //I need to iterate the previous current, which is now other
            if (_groupedEntityToAdd.AnyPreviousEntityCreated())
            {
                _cachedRangeOfSubmittedIndices.FastClear();
                using (sampler.Sample("Add operations"))
                {
                    try
                    {
                        using (sampler.Sample("Add entities to database"))
                        {
                            //each group is indexed by entity view type. for each type there is a dictionary indexed
                            //by entityID
                            foreach (var groupToSubmit in _groupedEntityToAdd)
                            {
                                var groupID = groupToSubmit.@group;
                                var groupDB = GetOrAddDBGroup(groupID);

                                //add the entityComponents in the group
                                foreach (var entityComponentsToSubmit in groupToSubmit.components)
                                {
                                    var type           = entityComponentsToSubmit.key;
                                    var fromDictionary = entityComponentsToSubmit.value;
                                    var wrapper        = new RefWrapperType(type);

                                    var toDictionary =
                                        GetOrAddTypeSafeDictionary(groupID, groupDB, wrapper, fromDictionary);

                                    //all the new entities are added at the end of each dictionary list, so we can
                                    //just iterate the list using the indices ranges added in the _cachedIndices
                                    _cachedRangeOfSubmittedIndices.Add(((uint, uint))(toDictionary.count,
                                        toDictionary.count + fromDictionary.count));
                                    //Fill the DB with the entity components generated this frame.
                                    fromDictionary.AddEntitiesToDictionary(toDictionary, groupID, entityLocator);
                                }
                            }
                        }

                        //then submit everything in the engines, so that the DB is up to date with all the entity components
                        //created by the entity built
                        var enumerator = _cachedRangeOfSubmittedIndices.GetEnumerator();
                        using (sampler.Sample("Add entities to engines fast"))
                        {
                            foreach (GroupInfo groupToSubmit in _groupedEntityToAdd)
                            {
                                var groupID = groupToSubmit.@group;
                                var groupDB = GetDBGroup(groupID);

                                foreach (var entityComponentsToSubmit in groupToSubmit.components)
                                {
                                    var type    = entityComponentsToSubmit.key;
                                    var wrapper = new RefWrapperType(type);

                                    var toDictionary = GetTypeSafeDictionary(groupID, groupDB, wrapper);
                                    enumerator.MoveNext();
                                    toDictionary.ExecuteEnginesAddEntityCallbacksFast(_reactiveEnginesAddEx, groupID,
                                        enumerator.Current, in sampler);
                                }
                            }
                        }

                        //then submit everything in the engines, so that the DB is up to date with all the entity components
                        //created by the entity built
                        using (sampler.Sample("Add entities to engines"))
                        {
                            foreach (GroupInfo groupToSubmit in _groupedEntityToAdd)
                            {
                                var groupID = groupToSubmit.@group;
                                var groupDB = GetDBGroup(groupID);

                                //This loop iterates again all the entity components that have been just submitted to call
                                //the Add Callbacks on them. Note that I am iterating the transient buffer of the just
                                //added components, but calling the callback on the entities just added in the real buffer
                                //Note: it's OK to add new entities while this happens because of the double buffer
                                //design of the transient buffer of added entities.
                                foreach (var entityComponentsToSubmit in groupToSubmit.components)
                                {
                                    var type           = entityComponentsToSubmit.key;
                                    var fromDictionary = entityComponentsToSubmit.value;

                                    //this contains the total number of components ever submitted in the DB
                                    ITypeSafeDictionary toDictionary = GetTypeSafeDictionary(groupID, groupDB, type);

                                    fromDictionary.ExecuteEnginesAddCallbacks(_reactiveEnginesAdd, toDictionary,
                                        groupID, in sampler);
                                }
                            }
                        }
                    }
                    finally
                    {
                        using (sampler.Sample("clear double buffering"))
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearLastAddOperations();
                        }
                    }
                }
            }
        }

        bool HasMadeNewStructuralChangesInThisIteration()
        {
            return _groupedEntityToAdd.AnyEntityCreated() || _entitiesOperations.AnyOperationQueued();
        }

        void RemoveEntitiesFromGroup(ExclusiveGroupStruct groupID, in PlatformProfiler profiler)
        {
            _entityLocator.RemoveAllGroupReferenceLocators(groupID);

            if (_groupEntityComponentsDB.TryGetValue(groupID, out var dictionariesOfEntities))
            {
                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    //RemoveEX happens inside
                    dictionaryOfEntities.value.ExecuteEnginesRemoveCallbacks_Group(_reactiveEnginesRemove, 
                        _reactiveEnginesRemoveEx, groupID, profiler);
                }

                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    dictionaryOfEntities.value.Clear();

                    _groupsPerEntity[dictionaryOfEntities.key][groupID].Clear();
                }
            }
        }

        void SwapEntitiesBetweenGroups(ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId,
            PlatformProfiler platformProfiler)
        {
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup = GetDBGroup(fromGroupId);
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup   = GetOrAddDBGroup(toGroupId);

            _entityLocator.UpdateAllGroupReferenceLocators(fromGroupId, toGroupId);

            //remove entities from dictionaries
            foreach (var dictionaryOfEntities in fromGroup)
            {
                RefWrapperType refWrapperType = dictionaryOfEntities.key;

                ITypeSafeDictionary fromDictionary = dictionaryOfEntities.value;
                ITypeSafeDictionary toDictionary =
                    GetOrAddTypeSafeDictionary(toGroupId, toGroup, refWrapperType, fromDictionary);

                fromDictionary.AddEntitiesToDictionary(toDictionary, toGroupId, this.entityLocator);
            }

            //Call all the callbacks
            foreach (var dictionaryOfEntities in fromGroup)
            {
                RefWrapperType refWrapperType = dictionaryOfEntities.key;

                ITypeSafeDictionary fromDictionary = dictionaryOfEntities.value;
                ITypeSafeDictionary toDictionary   = GetTypeSafeDictionary(toGroupId, toGroup, refWrapperType);

                //SwapEX happens inside
                fromDictionary.ExecuteEnginesSwapCallbacks_Group(_reactiveEnginesSwap, 
                    _reactiveEnginesSwapEx, toDictionary, fromGroupId, toGroupId, platformProfiler);
            }

            //remove entities from dictionaries
            foreach (var dictionaryOfEntities in fromGroup)
            {
                dictionaryOfEntities.value.Clear();

                _groupsPerEntity[dictionaryOfEntities.key][fromGroupId].Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ITypeSafeDictionary GetOrAddTypeSafeDictionary(ExclusiveGroupStruct groupId,
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> groupPerComponentType, RefWrapperType type,
            ITypeSafeDictionary fromDictionary)
        {
            //be sure that the TypeSafeDictionary for the entity Type exists
            if (groupPerComponentType.TryGetValue(type, out ITypeSafeDictionary toEntitiesDictionary) == false)
            {
                toEntitiesDictionary = fromDictionary.Create();
                groupPerComponentType.Add(type, toEntitiesDictionary);
            }

            {
                //update GroupsPerEntity
                if (_groupsPerEntity.TryGetValue(type, out var groupedGroup) == false)
                    groupedGroup = _groupsPerEntity[type] =
                        new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

                groupedGroup[groupId] = toEntitiesDictionary;
            }

            return toEntitiesDictionary;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ITypeSafeDictionary GetTypeSafeDictionary(ExclusiveGroupStruct groupID,
            FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group, RefWrapperType refWrapper)
        {
            if (@group.TryGetValue(refWrapper, out ITypeSafeDictionary fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no group found: ".FastConcat(groupID.ToName()));
            }

            return fromTypeSafeDictionary;
        }

        readonly DoubleBufferedEntitiesToAdd  _groupedEntityToAdd;
        readonly EntitiesOperations           _entitiesOperations;
        readonly FasterList<(uint, uint)>     _cachedRangeOfSubmittedIndices;
        readonly FasterDictionary<uint, uint> _cachedIndicesToSwapBeforeSubmissionForFilters;

        static readonly
            Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                    FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>, FasterList<(EGID, EGID)>
               ,
                EnginesRoot> _swapEntities;

        static readonly Action<
            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>,
            FasterList<EGID>, EnginesRoot> _removeEntities;

        static readonly Action<ExclusiveGroupStruct, EnginesRoot>                       _removeGroup;
        static readonly Action<ExclusiveGroupStruct, ExclusiveGroupStruct, EnginesRoot> _swapGroup;
    }
}