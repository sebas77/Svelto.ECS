using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        /// <summary>
        /// Todo: it would be probably better to split even further the logic between submission and callbacks
        /// Something to do when I will optimize the callbacks
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="maxNumberOfOperations"></param>
        IEnumerator<bool> SingleSubmission(PlatformProfiler profiler)
        {
            while (true)
            {
                DBC.ECS.Check.Require(_maxNumberOfOperationsPerFrame > 0);
                    
                ClearChecks();

                uint numberOfOperations   = 0;

                if (_entitiesOperations.count > 0)
                {
                    using (var sample = profiler.Sample("Remove and Swap operations"))
                    {
                        _transientEntitiesOperations.FastClear();
                        _entitiesOperations.CopyValuesTo(_transientEntitiesOperations);
                        _entitiesOperations.FastClear();

                        EntitySubmitOperation[] entitiesOperations =
                            _transientEntitiesOperations.ToArrayFast(out var count);
                        
                        for (var i = 0; i < count; i++)
                        {
                            try
                            {
                                switch (entitiesOperations[i].type)
                                {
                                    case EntitySubmitOperationType.Swap:
                                        MoveEntityFromAndToEngines(entitiesOperations[i].builders
                                                                 , entitiesOperations[i].fromID
                                                                 , entitiesOperations[i].toID);
                                        break;
                                    case EntitySubmitOperationType.Remove:
                                        MoveEntityFromAndToEngines(entitiesOperations[i].builders
                                                                 , entitiesOperations[i].fromID, null);
                                        break;
                                    case EntitySubmitOperationType.RemoveGroup:
                                        RemoveEntitiesFromGroup(entitiesOperations[i].fromID.groupID, profiler);
                                        break;
                                    case EntitySubmitOperationType.SwapGroup:
                                        SwapEntitiesBetweenGroups(entitiesOperations[i].fromID.groupID
                                                                , entitiesOperations[i].toID.groupID, profiler);
                                        break;
                                }
                            }
                            catch
                            {
                                var str = "Crash while executing Entity Operation ".FastConcat(
                                    entitiesOperations[i].type.ToString());

                                Svelto.Console.LogError(str.FastConcat(" ")
#if DEBUG && !PROFILE_SVELTO
                                                      .FastConcat(entitiesOperations[i].trace.ToString())
#endif
                                );

                                throw;
                            }

                            ++numberOfOperations;

                            if ((uint) numberOfOperations >= (uint) _maxNumberOfOperationsPerFrame)
                            {
                                using (sample.Yield())
                                yield return true;

                                numberOfOperations = 0;
                            }
                        }
                    }
                }

                _groupedEntityToAdd.Swap();

                if (_groupedEntityToAdd.AnyOtherEntityCreated())
                {
                    using (var outerSampler = profiler.Sample("Add operations"))
                    {
                        try
                        {
                            using (profiler.Sample("Add entities to database"))
                            {
                                //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
                                foreach (var groupToSubmit in _groupedEntityToAdd.other)
                                {
                                    var groupID = new ExclusiveGroupStruct(groupToSubmit.Key);
                                    var groupDB = GetOrCreateDBGroup(groupID);

                                    //add the entityComponents in the group
                                    foreach (var entityComponentsToSubmit in groupToSubmit.Value)
                                    {
                                        var type                     = entityComponentsToSubmit.Key;
                                        var targetTypeSafeDictionary = entityComponentsToSubmit.Value;
                                        var wrapper                  = new RefWrapperType(type);

                                        var dbDic = GetOrCreateTypeSafeDictionary(
                                            groupID, groupDB, wrapper, targetTypeSafeDictionary);

                                        //Fill the DB with the entity components generated this frame.
                                        dbDic.AddEntitiesFromDictionary(targetTypeSafeDictionary, (uint) groupID, this);
                                    }
                                }
                            }

                            //then submit everything in the engines, so that the DB is up to date with all the entity components
                            //created by the entity built
                            using (var sampler = profiler.Sample("Add entities to engines"))
                            {
                                foreach (var groupToSubmit in _groupedEntityToAdd.other)
                                {
                                    var groupID = new ExclusiveGroupStruct(groupToSubmit.Key);
                                    var groupDB = GetDBGroup(groupID);
//entityComponentsToSubmit is the array of components found in the groupID per component type. 
//if there are N entities to submit, and M components type to add for each entity, this foreach will run NxM times. 
                                    foreach (var entityComponentsToSubmit in groupToSubmit.Value)
                                    {
                                        var realDic = groupDB[new RefWrapperType(entityComponentsToSubmit.Key)];

                                        entityComponentsToSubmit.Value.ExecuteEnginesAddOrSwapCallbacks(
                                            _reactiveEnginesAddRemove, realDic, null, new ExclusiveGroupStruct(groupID)
                                          , in profiler);

                                        numberOfOperations += entityComponentsToSubmit.Value.count;

                                        if (numberOfOperations >= _maxNumberOfOperationsPerFrame)
                                        {
                                            using (outerSampler.Yield())
                                            using (sampler.Yield())
                                            {
                                                yield return true;
                                            }

                                            numberOfOperations = 0;
                                        }
                                    }
                                }
                            }
                        }
                        finally
                        {
                            using (profiler.Sample("clear double buffering"))
                            {
                                //other can be cleared now, but let's avoid deleting the dictionary every time
                                _groupedEntityToAdd.ClearOther();
                            }
                        }
                    }
                }

                yield return false;
            }
        }
        
        bool HasMadeNewStructuralChangesInThisIteration()
        {
            return _groupedEntityToAdd.AnyEntityCreated() || _entitiesOperations.count > 0;
        }

        readonly DoubleBufferedEntitiesToAdd                    _groupedEntityToAdd;
        readonly FasterDictionary<ulong, EntitySubmitOperation> _entitiesOperations;
        readonly FasterList<EntitySubmitOperation>              _transientEntitiesOperations;
        uint                                                    _maxNumberOfOperationsPerFrame;
    }
}