using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        readonly FasterList<EntitySubmitOperation> _transientEntitiesOperations;

        void SubmitEntityComponents()
        {
            using (var profiler = new PlatformProfiler("Svelto.ECS - Entities Submission"))
            {
                int iterations = 0;
                do
                {
                    SingleSubmission(profiler);
                } while ((_groupedEntityToAdd.currentEntitiesCreatedPerGroup.count > 0 ||
                          _entitiesOperations.Count > 0) && ++iterations < 5);

#if DEBUG && !PROFILE_SVELTO
                if (iterations == 5)
                    throw new ECSException("possible circular submission detected");
#endif
            }
        }

        void SingleSubmission(in PlatformProfiler profiler)
        {
#if UNITY_ECS            
            NativeOperationSubmission(profiler);
#endif
            
            if (_entitiesOperations.Count > 0)
            {
                using (profiler.Sample("Remove and Swap operations"))
                {
                    _transientEntitiesOperations.FastClear();
                    _entitiesOperations.CopyValuesTo(_transientEntitiesOperations);
                    _entitiesOperations.FastClear();

                    EntitySubmitOperation[] entitiesOperations = _transientEntitiesOperations.ToArrayFast(out var count);
                    for (var i = 0; i < count; i++)
                    {
                        try
                        {
                            switch (entitiesOperations[i].type)
                            {
                                case EntitySubmitOperationType.Swap:
                                    MoveEntityFromAndToEngines(entitiesOperations[i].builders,
                                        entitiesOperations[i].fromID, entitiesOperations[i].toID);
                                    break;
                                case EntitySubmitOperationType.Remove:
                                    MoveEntityFromAndToEngines(entitiesOperations[i].builders,
                                        entitiesOperations[i].fromID, null);
                                    break;
                                case EntitySubmitOperationType.RemoveGroup:
                                    RemoveGroupAndEntities(
                                        entitiesOperations[i].fromID.groupID, profiler);
                                    break;
                                case EntitySubmitOperationType.SwapGroup:
                                    SwapEntitiesBetweenGroups(entitiesOperations[i].fromID.groupID,
                                        entitiesOperations[i].toID.groupID, profiler);
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            var str = "Crash while executing Entity Operation "
                                .FastConcat(entitiesOperations[i].type.ToString());

                            throw new ECSException(str.FastConcat(" ")
#if DEBUG && !PROFILE_SVELTO
                                    .FastConcat(entitiesOperations[i].trace.ToString())
#endif
                                , e);
                        }
                    }
                }
            }

            _groupedEntityToAdd.Swap();

            if (_groupedEntityToAdd.otherEntitiesCreatedPerGroup.count > 0)
            {
                using (profiler.Sample("Add operations"))
                {
                    try
                    {
                        AddEntityComponentsToTheDBAndSuitableEngines(profiler);
                    }
                    finally
                    {
                        using (profiler.Sample("clear 6operates double buffering"))
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearOther();
                        }
                    }
                }
            }
        }

        void AddEntityComponentsToTheDBAndSuitableEngines(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Add entities to database"))
            {
                //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
                foreach (var groupOfEntitiesToSubmit in _groupedEntityToAdd.otherEntitiesCreatedPerGroup)
                {
                    var groupID = groupOfEntitiesToSubmit.Key;
                    
                    FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> groupDB = GetOrCreateGroup(groupID);

                    //add the entityComponents in the group
                    foreach (var entityComponentsToSubmit in _groupedEntityToAdd.other[groupID])
                    {
                        var type = entityComponentsToSubmit.Key;
                        var targetTypeSafeDictionary = entityComponentsToSubmit.Value;
                        var wrapper = new RefWrapper<Type>(type);

                        ITypeSafeDictionary dbDic = GetOrCreateTypeSafeDictionary(groupID, groupDB, wrapper, 
                            targetTypeSafeDictionary);

                        //Fill the DB with the entity views generate this frame.
                        dbDic.AddEntitiesFromDictionary(targetTypeSafeDictionary, groupID);
                    }
                }
            }

            //then submit everything in the engines, so that the DB is up to date with all the entity views and struct
            //created by the entity built
            using (profiler.Sample("Add entities to engines"))
            {
                foreach (var groupToSubmit in _groupedEntityToAdd.otherEntitiesCreatedPerGroup)
                {
                    var groupID = groupToSubmit.Key;
                    var groupDB = _groupEntityComponentsDB[groupID];

                    foreach (var entityComponentsToSubmit in _groupedEntityToAdd.other[groupID])
                    {
                        var realDic = groupDB[new RefWrapper<Type>(entityComponentsToSubmit.Key)];

                        entityComponentsToSubmit.Value.AddEntitiesToEngines(_reactiveEnginesAddRemove, realDic,
                            new ExclusiveGroupStruct(groupToSubmit.Key), in profiler);
                    }
                }
            }
        }

        DoubleBufferedEntitiesToAdd                                 _groupedEntityToAdd;
        readonly ThreadSafeDictionary<ulong, EntitySubmitOperation> _entitiesOperations;
    }
}