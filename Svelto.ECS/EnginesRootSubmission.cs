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
            bool newEntityViewsHaveBeenAddedWhileIterating =  _groupedEntityViewsToAdd.current.Count > 0;

            int numberOfReenteringLoops = 0;

            while (newEntityViewsHaveBeenAddedWhileIterating)
            {
                //use other as source from now on
                //current will be use to write new entityViews
                _groupedEntityViewsToAdd.Swap();

                if (_groupedEntityViewsToAdd.other.Count > 0)
                    AddEntityViewsToTheDBAndSuitableEngines(_groupedEntityViewsToAdd.other);

                //other can be cleared now
                _groupedEntityViewsToAdd.other.Clear();

                //has current new entityViews?
                newEntityViewsHaveBeenAddedWhileIterating = _groupedEntityViewsToAdd.current.Count > 0;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside IEntityViewsEngine Add method, please consider building entities outside IEntityViewsEngine Add method");

                numberOfReenteringLoops++;
            }
        }
        
        //todo: groupsToSubmit can be semplified as data structure?
        void AddEntityViewsToTheDBAndSuitableEngines(ITypeSafeDictionary<int, Dictionary<Type, ITypeSafeDictionary>> groupsOfEntitiesToSubmit)
        {
            //for each groups there is a dictionary of built lists of EntityView grouped by type
            foreach (var groupOfEntitiesToSubmit in groupsOfEntitiesToSubmit)
            {
                Dictionary<Type, ITypeSafeDictionary> groupDB;
                int groupID = groupOfEntitiesToSubmit.Key;

                //if the group doesn't exist in the current DB let's create it first
                if (_groupEntityViewsDB.TryGetValue(groupID, out groupDB) == false)
                    groupDB = _groupEntityViewsDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

                //add the entity View in the group
                foreach (var entityViewList in groupOfEntitiesToSubmit.Value)
                {
                    ITypeSafeDictionary dbList;
                    if (groupDB.TryGetValue(entityViewList.Key, out dbList) == false)
                        dbList = groupDB[entityViewList.Key] = entityViewList.Value.Create();

                    dbList.FillWithIndexedEntityViews(entityViewList.Value);
                }
            }

            //then submit everything in the engines, so that the DB is up to date
            //with all the entity views and struct created by the entity built
            foreach (var groupToSubmit in groupsOfEntitiesToSubmit)
            {    
                foreach (var entityViewsPerType in groupToSubmit.Value)
                {
                    var type = entityViewsPerType.Key;
                    for (var current = type;
                         current != _entityViewType && current != _objectType && current != _valueType;
                         current = current.BaseType)
                            entityViewsPerType.Value.AddEntityViewsToEngines(_entityViewEngines);
                }
            }
        }
        
        readonly DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>> _groupedEntityViewsToAdd;
        readonly EntitySubmissionScheduler _scheduler;
    }
}