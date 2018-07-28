using System;
using System.Collections.Generic;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable
    {
        void SubmitEntityViews()
        {
            int numberOfReenteringLoops = 0;

            //are there new entities built to process?
            while ( _newEntitiesBuiltToProcess > 0)
            {
                //use other as source from now on
                //current will be use to write new entityViews
                _groupedEntityToAdd.Swap();

                //Note: if N entity of the same type are added on the same frame
                //the Add callback is called N times on the same frame.
                //if the Add calback builds a new entity, that entity will not
                //be available in the database until the N callbacks are done
                //solving it could be complicated as callback and database update
                //must be interleaved.
                if (_groupedEntityToAdd.other.Count > 0)
                    AddEntityViewsToTheDBAndSuitableEngines(_groupedEntityToAdd.other);

                //other can be cleared now, but let's avoid deleting the dictionary every time
                _groupedEntityToAdd.ClearOther();

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside IEntityViewsEngine Add method, please consider building entities outside IEntityViewsEngine Add method");

                _newEntitiesBuiltToProcess = 0;
                numberOfReenteringLoops++;
            }
        }
        
        //todo: groupsToSubmit can be simplified as data structure?
        void AddEntityViewsToTheDBAndSuitableEngines(Dictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupsOfEntitiesToSubmit)
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
                    
                    if (_groupedGroups.TryGetValue(entityViewTypeSafeDictionary.Key, out groupedGroup) == false)
                        groupedGroup = _groupedGroups[entityViewTypeSafeDictionary.Key] = new FasterDictionary<int, ITypeSafeDictionary>();

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
        
        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks 
        //to the FasterDictionary capabilities OR it's possible to get a specific entityView indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        
        readonly Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>       _groupEntityDB;
        readonly Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> _groupedGroups; //yes I am being sarcastic
        readonly DoubleBufferedEntitiesToAdd<Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>> _groupedEntityToAdd;
        readonly EntitySubmissionScheduler                                    _scheduler;
    }
}