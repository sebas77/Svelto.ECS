using System;
using System.Collections.Generic;
using Svelto.DataStructures;
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
        //todo: can I make the entity creation less complicated?
        void AddEntityViewsToTheDBAndSuitableEngines(Dictionary<int, Dictionary<Type, ITypeSafeList>> groupsToSubmit)
        {
            //for each groups there is a dictionary of built lists of EntityView grouped by type
            foreach (var groupToSubmit in groupsToSubmit)
            {
                Dictionary<Type, ITypeSafeList> groupDB;
                int groupID = groupToSubmit.Key;

                //if the group doesn't exist in the current DB let's create it frst
                if (_groupEntityViewsDB.TryGetValue(groupID, out groupDB) == false)
                    groupDB = _groupEntityViewsDB[groupID] = new Dictionary<Type, ITypeSafeList>();

                foreach (var entityViewsPerType in groupToSubmit.Value)
                {
                    //add the entity View in the group
                    if (entityViewsPerType.Value.isQueryiableEntityView == true)
                        AddEntityViewToDB(groupDB, entityViewsPerType);
                    //add the entity view in the gloal pool
                    AddEntityViewToDB(_globalEntityViewsDB, entityViewsPerType);
                    //and it's not a struct, add in the indexable DB too
                    AddEntityViewToEntityViewsDictionary(_globalEntityViewsDBDic, entityViewsPerType.Value, entityViewsPerType.Key);
                }
            }

            //then submit everything in the engines, so that the DB is up to date
            //with all the entity views and struct created by the entity built
            foreach (var group in groupsToSubmit)
            {
                foreach (var entityViewList in group.Value)
                {
                    if (entityViewList.Value.isQueryiableEntityView)
                    {
                        var type = entityViewList.Key;
                        for (var current = type; current != _entityViewType; current = current.BaseType)
                            AddEntityViewToTheSuitableEngines(_entityViewEngines, entityViewList.Value, current);
                    }
                }
            }
        }

        static void AddEntityViewToDB(  Dictionary<Type, ITypeSafeList> entityViewsDB, 
                                        KeyValuePair<Type, ITypeSafeList> entityViewList)
        {
            
            {
                ITypeSafeList dbList;

                if (entityViewsDB.TryGetValue(entityViewList.Key, out dbList) == false)
                    dbList = entityViewsDB[entityViewList.Key] = entityViewList.Value.Create();

                dbList.AddRange(entityViewList.Value);
            }
        }

        static void AddEntityViewToEntityViewsDictionary(Dictionary<Type, ITypeSafeDictionary> entityViewsDBdic,
                                                        ITypeSafeList entityViews, Type entityViewType)
        {
            if (entityViews.isQueryiableEntityView == true)
            {
                ITypeSafeDictionary entityViewsDic;
    
                if (entityViewsDBdic.TryGetValue(entityViewType, out entityViewsDic) == false)
                    entityViewsDic = entityViewsDBdic[entityViewType] = entityViews.CreateIndexedDictionary();
    
                entityViewsDic.FillWithIndexedEntityViews(entityViews);
            }
        }

        static void AddEntityViewToTheSuitableEngines(Dictionary<Type, FasterList<IHandleEntityViewEngine>> entityViewEngines, ITypeSafeList entityViewsList, Type entityViewType)
        {
            FasterList<IHandleEntityViewEngine> enginesForEntityView;

            if (entityViewEngines.TryGetValue(entityViewType, out enginesForEntityView))
            {
                int viewsCount;

                var entityViews = entityViewsList.ToArrayFast(out viewsCount);

                for (int i = 0; i < viewsCount; i++)
                {
                    int count;
                    var fastList = FasterList<IHandleEntityViewEngine>.NoVirt.ToArrayFast(enginesForEntityView, out count);
                    IEntityView entityView = entityViews[i];
                    for (int j = 0; j < count; j++)
                    {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                        EngineProfiler.MonitorAddDuration(fastList[j], entityView);
#else
                        fastList[j].Add(entityView);
#endif
                    }
                }
            }
        }
        
        readonly DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeList>>> _groupedEntityViewsToAdd;
        readonly EntitySubmissionScheduler _scheduler;
    }
}