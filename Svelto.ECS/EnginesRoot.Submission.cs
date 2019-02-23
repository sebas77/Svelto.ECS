using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void SubmitEntityViews()
        {
            var profiler = new PlatformProfiler();
            using (profiler.StartNewSession("Svelto.ECS - Entities Submission"))
            {
                if (_entitiesOperations.Count > 0)
                {
                    using (profiler.Sample("Remove and Swap operations"))
                    {
#if DEBUG && !PROFILER
                        _entitiesOperationsDebug.Clear();
#endif
                        _transientEntitiesOperations.FastClear();
                        _transientEntitiesOperations.AddRange(_entitiesOperations);
                        _entitiesOperations.FastClear();

                        var entitiesOperations = _transientEntitiesOperations.ToArrayFast();
                        for (var i = 0; i < _transientEntitiesOperations.Count; i++)
                        {
                            try
                            {
                                switch (entitiesOperations[i].type)
                                {
                                    case EntitySubmitOperationType.Swap:
                                        SwapEntityGroup(entitiesOperations[i].builders,
                                                        entitiesOperations[i].entityDescriptor,
                                                        new EGID(entitiesOperations[i].ID,
                                                                 entitiesOperations[i].fromGroupID),
                                                        new EGID(entitiesOperations[i].toID,
                                                                 entitiesOperations[i].toGroupID));
                                        break;
                                    case EntitySubmitOperationType.Remove:
                                        MoveEntity(entitiesOperations[i].builders,
                                                   new EGID(entitiesOperations[i].ID,
                                                            entitiesOperations[i].fromGroupID),
                                                   entitiesOperations[i].entityDescriptor, new EGID());
                                        break;
                                    case EntitySubmitOperationType.RemoveGroup:
                                        if (entitiesOperations[i].entityDescriptor == null)
                                            RemoveGroupAndEntitiesFromDB(entitiesOperations[i].fromGroupID);
                                        else
                                            RemoveGroupAndEntitiesFromDB(entitiesOperations[i].fromGroupID,
                                                                         entitiesOperations[i].entityDescriptor);

                                        break;
                                }
                            }
                            catch (Exception e)
                            {
#if DEBUG && !PROFILER
                                var str = "Crash while executing Entity Operation"
                                   .FastConcat(entitiesOperations[i].type.ToString()).FastConcat(" id: ")
                                   .FastConcat(entitiesOperations[i].ID).FastConcat(" to id: ")
                                   .FastConcat(entitiesOperations[i].toID).FastConcat(" from groupid: ")
                                   .FastConcat(entitiesOperations[i].fromGroupID).FastConcat(" to groupid: ")
                                   .FastConcat(entitiesOperations[i].toGroupID);
#if RELAXED_ECS
                                Console.LogException(str.FastConcat(" ", entitiesOperations[i].trace), e);
#else
                                throw new ECSException(str.FastConcat(" ").FastConcat(entitiesOperations[i].trace), e);
#endif
#else
                                var str = "Entity Operation is ".FastConcat(entitiesOperations[i].type.ToString())
                                                                .FastConcat(" id: ")
                                                                .FastConcat(entitiesOperations[i].ID)
                                                                .FastConcat(" to id: ")
                                                                .FastConcat(entitiesOperations[i].toID)
                                                                .FastConcat(" from groupid: ")
                                                                .FastConcat(entitiesOperations[i].fromGroupID)
                                                                .FastConcat(" to groupid: ")
                                                                .FastConcat(entitiesOperations[i].toGroupID);

                                Console.LogException(str, e);
#endif
                            }
                        }
                    }
                }

                if (_groupedEntityToAdd.current.Count > 0)
                {
                    using (profiler.Sample("Add operations"))
                    {
                        //use other as source from now on current will be use to write new entityViews
                        _groupedEntityToAdd.Swap();

                        try
                        {
                            //Note: if N entity of the same type are added on the same frame the Add callback is called N
                            //times on the same frame. if the Add callback builds a new entity, that entity will not
                            //be available in the database until the N callbacks are done solving it could be complicated as
                            //callback and database update must be interleaved.
                            AddEntityViewsToTheDBAndSuitableEngines(_groupedEntityToAdd.other, profiler);
                        }
#if !DEBUG
                        catch (Exception e)
                        {
                            Console.LogException(e);
                        }
#endif
                        finally
                        {
                            //other can be cleared now, but let's avoid deleting the dictionary every time
                            _groupedEntityToAdd.ClearOther();
                        }
                    }
                }
            }
        }

        void AddEntityViewsToTheDBAndSuitableEngines(
            FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupsOfEntitiesToSubmit,
            PlatformProfiler profiler)
        {
            //each group is indexed by entity view type. for each type there is a dictionary indexed by entityID
            foreach (var groupOfEntitiesToSubmit in groupsOfEntitiesToSubmit)
            {
                Dictionary<Type, ITypeSafeDictionary> groupDB;
                int groupID = groupOfEntitiesToSubmit.Key;

                //if the group doesn't exist in the current DB let's create it first
                if (_groupEntityDB.TryGetValue(groupID, out groupDB) == false)
                    groupDB = _groupEntityDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

                //add the entityViews in the group
                foreach (var entityViewTypeSafeDictionary in groupOfEntitiesToSubmit.Value)
                {
                    ITypeSafeDictionary dbDic;
                    FasterDictionary<int, ITypeSafeDictionary> groupedGroup = null;
                    if (groupDB.TryGetValue(entityViewTypeSafeDictionary.Key, out dbDic) == false)
                        dbDic = groupDB[entityViewTypeSafeDictionary.Key] = entityViewTypeSafeDictionary.Value.Create();

                    if (_groupsPerEntity.TryGetValue(entityViewTypeSafeDictionary.Key, out groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[entityViewTypeSafeDictionary.Key] =
                            new FasterDictionary<int, ITypeSafeDictionary>();

                    //Fill the DB with the entity views generate this frame.
                    dbDic.FillWithIndexedEntities(entityViewTypeSafeDictionary.Value);
                    groupedGroup[groupID] = dbDic;
                }
            }

            //then submit everything in the engines, so that the DB is up to date
            //with all the entity views and struct created by the entity built
            foreach (var groupToSubmit in groupsOfEntitiesToSubmit)
            {
                foreach (var entityViewsPerType in groupToSubmit.Value)
                {
                    using (profiler.Sample("Add entities to engines"))
                    {
                        entityViewsPerType.Value.AddEntitiesToEngines(_entityEngines, ref profiler);
                    }
                }
            }
        }

        readonly DoubleBufferedEntitiesToAdd<FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>>
            _groupedEntityToAdd;

        readonly IEntitySubmissionScheduler        _scheduler;
        readonly FasterList<EntitySubmitOperation> _transientEntitiesOperations;
        readonly FasterList<EntitySubmitOperation> _entitiesOperations;
    }
}