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
            bool newEntityViewsHaveBeenAddedWhileIterating =
                _metaEntityViewsToAdd.current.Count > 0
                || _entityViewsToAdd.current.Count > 0
                || _groupedEntityViewsToAdd.current.Count > 0;

            int numberOfReenteringLoops = 0;

            while (newEntityViewsHaveBeenAddedWhileIterating)
            {
                //use other as source from now on
                //current will be use to write new entityViews
                _entityViewsToAdd.Swap();
                _metaEntityViewsToAdd.Swap();
                _groupedEntityViewsToAdd.Swap();

                if (_entityViewsToAdd.other.Count > 0)
                    AddEntityViewsToTheDBAndSuitableEngines(_entityViewsToAdd.other, _entityViewsDB);

                if (_metaEntityViewsToAdd.other.Count > 0)
                    AddEntityViewsToTheDBAndSuitableEngines(_metaEntityViewsToAdd.other, _metaEntityViewsDB);

                if (_groupedEntityViewsToAdd.other.Count > 0)
                    AddGroupEntityViewsToTheDBAndSuitableEngines(_groupedEntityViewsToAdd.other, _groupEntityViewsDB, _entityViewsDB);

                //other can be cleared now
                _entityViewsToAdd.other.Clear();
                _metaEntityViewsToAdd.other.Clear();
                _groupedEntityViewsToAdd.other.Clear();

                //has current new entityViews?
                newEntityViewsHaveBeenAddedWhileIterating =
                    _metaEntityViewsToAdd.current.Count > 0
                    || _entityViewsToAdd.current.Count > 0
                    || _groupedEntityViewsToAdd.current.Count > 0;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside IEntityViewsEngine Add method, please consider building entities outside IEntityViewsEngine Add method");

                numberOfReenteringLoops++;
            }
        }

        void AddEntityViewsToTheDBAndSuitableEngines(Dictionary<Type, ITypeSafeList> entityViewsToAdd,
            Dictionary<Type, ITypeSafeList> entityViewsDB)
        {
            foreach (var entityViewList in entityViewsToAdd)
            {
                AddEntityViewToDB(entityViewsDB, entityViewList);

                if (entityViewList.Value.isQueryiableEntityView)
                {
                    AddEntityViewToEntityViewsDictionary(_entityViewsDBDic, entityViewList.Value, entityViewList.Key);
                }
            }

            foreach (var entityViewList in entityViewsToAdd)
            {
                if (entityViewList.Value.isQueryiableEntityView)
                {
                    var type = entityViewList.Key;
                    for (var current = type; current != _entityViewType; current = current.BaseType)
                        AddEntityViewToTheSuitableEngines(_entityViewEngines, entityViewList.Value,
                                                          current);
                }
            }
        }

        void AddGroupEntityViewsToTheDBAndSuitableEngines(Dictionary<int, Dictionary<Type, ITypeSafeList>> groupedEntityViewsToAdd,
                                                      Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsDB,
                                                      Dictionary<Type, ITypeSafeList> entityViewsDB)
        {
            foreach (var group in groupedEntityViewsToAdd)
            {
                AddEntityViewsToGroupDB(groupEntityViewsDB, @group);

                AddEntityViewsToTheDBAndSuitableEngines(group.Value, entityViewsDB);
            }
        }

        static void AddEntityViewsToGroupDB(Dictionary<int, Dictionary<Type, ITypeSafeList>> groupEntityViewsDB,
                                      KeyValuePair<int, Dictionary<Type, ITypeSafeList>> @group)
        {
            Dictionary<Type, ITypeSafeList> groupedEntityViewsByType;

            if (groupEntityViewsDB.TryGetValue(@group.Key, out groupedEntityViewsByType) == false)
                groupedEntityViewsByType = groupEntityViewsDB[@group.Key] = new Dictionary<Type, ITypeSafeList>();

            foreach (var entityView in @group.Value)
            {
                groupedEntityViewsByType.Add(entityView.Key, entityView.Value);
            }
        }

        static void AddEntityViewToDB(Dictionary<Type, ITypeSafeList> entityViewsDB, KeyValuePair<Type, ITypeSafeList> entityViewList)
        {
            ITypeSafeList dbList;

            if (entityViewsDB.TryGetValue(entityViewList.Key, out dbList) == false)
                dbList = entityViewsDB[entityViewList.Key] = entityViewList.Value.Create();

            dbList.AddRange(entityViewList.Value);
        }

        static void AddEntityViewToEntityViewsDictionary(Dictionary<Type, ITypeSafeDictionary> entityViewsDBdic,
                                             ITypeSafeList entityViews, Type entityViewType)
        {
            ITypeSafeDictionary entityViewsDic;

            if (entityViewsDBdic.TryGetValue(entityViewType, out entityViewsDic) == false)
                entityViewsDic = entityViewsDBdic[entityViewType] = entityViews.CreateIndexedDictionary();

            entityViewsDic.FillWithIndexedEntityViews(entityViews);
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
        
        readonly DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>> _entityViewsToAdd;
        readonly DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>> _metaEntityViewsToAdd;
        readonly DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeList>>> _groupedEntityViewsToAdd;
      
        readonly EntitySubmissionScheduler _scheduler;

    }
}