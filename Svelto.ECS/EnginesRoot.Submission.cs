using System;
using System.Collections.Generic;
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
            bool newEntityViewsHaveBeenAddedWhileIterating =  _groupedEntityToAdd.current.Count > 0;

            int numberOfReenteringLoops = 0;

            while (newEntityViewsHaveBeenAddedWhileIterating)
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

                //other can be cleared now
                _groupedEntityToAdd.other.Clear();

                //has current new entityViews?
                newEntityViewsHaveBeenAddedWhileIterating = _groupedEntityToAdd.current.Count > 0;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside IEntityViewsEngine Add method, please consider building entities outside IEntityViewsEngine Add method");

                numberOfReenteringLoops++;
            }
        }
        
        //todo: groupsToSubmit can be semplified as data structure?
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
                    if (groupDB.TryGetValue(entityViewTypeSafeDictionary.Key, out dbDic) == false)
                        dbDic = groupDB[entityViewTypeSafeDictionary.Key] = entityViewTypeSafeDictionary.Value.Create();

                    //type safe copy
                    dbDic.FillWithIndexedEntityViews(entityViewTypeSafeDictionary.Value);
                }
            }

            //then submit everything in the engines, so that the DB is up to date
            //with all the entity views and struct created by the entity built
            foreach (var groupToSubmit in groupsOfEntitiesToSubmit)
            {    
                foreach (var entityViewsPerType in groupToSubmit.Value)
                {
                    entityViewsPerType.Value.AddEntityViewsToEngines(_entityEngines);
                }
            }
        }
        
        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks 
        //to the FasterDictionary capabilitiies OR it's possible to get a specific entityView indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        readonly DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>> _groupedEntityToAdd;
        readonly EntitySubmissionScheduler _scheduler;
    }
}