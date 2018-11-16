using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;
using Console = Svelto.Utilities.Console;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        void SubmitEntityViews()
        {
            using (new PlatformProfiler("Svelto.ECS").Sample("Entities submit"))
            {
                if (_entitiesOperations.Count > 0)
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
                                                    entitiesOperations[i].entityDescriptor,    entitiesOperations[i].id,
                                                    entitiesOperations[i].fromGroupID, entitiesOperations[i].toGroupID);
                                    break;
                                case EntitySubmitOperationType.Remove:
                                    MoveEntity(entitiesOperations[i].builders,
                                               new EGID(entitiesOperations[i].id, entitiesOperations[i].fromGroupID),
                                               entitiesOperations[i].entityDescriptor);
                                    break;
                                case EntitySubmitOperationType.RemoveGroup:
                                    RemoveGroupAndEntitiesFromDB(entitiesOperations[i].fromGroupID);
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
#if DEBUG && !PROFILER                        
                            var str = "Entity Operation is ".FastConcat(entitiesOperations[i].type.ToString())
                                               .FastConcat(" id: ")
                                               .FastConcat(entitiesOperations[i].id)
                                               .FastConcat(" from groupid: ")
                                               .FastConcat(entitiesOperations[i].fromGroupID)
                                               .FastConcat(" to groupid: ")
                                               .FastConcat(entitiesOperations[i].toGroupID);

                            Console.LogError(e.Message.FastConcat(" ", str, " ", entitiesOperations[i].trace));
#else
                            Console.LogException(e);
#endif
                        }
                    }
                }

                try
                {
                    if (_groupedEntityToAdd.current.Count > 0)
                    {
                        //use other as source from now on current will be use to write new entityViews
                        _groupedEntityToAdd.Swap();

                        //Note: if N entity of the same type are added on the same frame the Add callback is called N
                        //times on the same frame. if the Add callback builds a new entity, that entity will not
                        //be available in the database until the N callbacks are done solving it could be complicated as
                        //callback and database update must be interleaved.
                        AddEntityViewsToTheDBAndSuitableEngines(_groupedEntityToAdd.other);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e);
                }
                finally
                {
                    //other can be cleared now, but let's avoid deleting the dictionary every time
                    _groupedEntityToAdd.ClearOther();                    
                }
            }
        }
        
         void AddEntityViewsToTheDBAndSuitableEngines(
             FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupsOfEntitiesToSubmit)
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
                    entityViewsPerType.Value.AddEntitiesToEngines(_entityEngines);
                }
            }
        }

        readonly FasterList<EntitySubmitOperation> _entitiesOperations;
        
        readonly DoubleBufferedEntitiesToAdd<FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>>
            _groupedEntityToAdd;

        readonly IEntitySubmissionScheduler                                   _scheduler;
        readonly FasterList<EntitySubmitOperation>                            _transientEntitiesOperations;
    }
}