using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;
using Svelto.WeakEvents;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable
    {
        static EnginesRoot()
        {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
/// <summary>
/// I still need to find a good solution for this. Need to move somewhere else
/// </summary>
            UnityEngine.GameObject debugEngineObject = new UnityEngine.GameObject("Svelto.ECS.Profiler");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
            UnityEngine.GameObject.DontDestroyOnLoad(debugEngineObject);
#endif
        }
       
        /// <summary>
        /// Engines root contextualize your engines and entities. You don't need to limit yourself to one EngineRoot
        /// as multiple engines root could promote separation of scopes. The EntitySubmissionScheduler checks
        /// periodically if new entity must be submited to the database and the engines. It's an external
        /// dependencies to be indipendent by the running platform as the user can define it.
        /// The EntitySubmissionScheduler cannot hold an EnginesRoot reference, that's why
        /// it must receive a weak reference of the EnginesRoot callback.
        /// </summary>
        public EnginesRoot(EntitySubmissionScheduler entityViewScheduler)
        {
            _entityEngines = new Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>();
            _otherEngines = new FasterList<IEngine>();

            _groupEntityDB = new Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>();
            _groupEntityDB[ExclusiveGroups.StandardEntity] = new Dictionary<Type, ITypeSafeDictionary>();
            
            _groupedEntityToAdd = new DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeDictionary>>>();

            _DB = new entitiesDB(_groupEntityDB);

            _scheduler = entityViewScheduler;
            _scheduler.Schedule(new WeakAction(SubmitEntityViews));
        }

        public void AddEngine(IEngine engine)
        {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            Profiler.EngineProfiler.AddEngine(engine);
#endif
            var viewEngine = engine as IHandleEntityViewEngineAbstracted;
            
            if (viewEngine != null)
                CheckEntityViewsEngine(viewEngine);
            else            
                _otherEngines.Add(engine);
            
            var queryableEntityViewEngine = engine as IQueryingEntitiesEngine;
            if (queryableEntityViewEngine != null)
            {
                queryableEntityViewEngine.entitiesDB = _DB;
                queryableEntityViewEngine.Ready();
            }
        }
       
        void CheckEntityViewsEngine(IEngine engine)
        {
            var baseType = engine.GetType().GetBaseType();

            while (baseType != _objectType)
            {
                if (baseType.IsGenericTypeEx())
                {
                    var genericArguments = baseType.GetGenericArgumentsEx();
                    
                    AddEngine(engine as IHandleEntityViewEngineAbstracted, genericArguments, _entityEngines);

                    return;
                }

                baseType = baseType.GetBaseType();
            }

            throw new ArgumentException("Not Supported Engine " + engine.ToString());
        }

        //The T parameter allows to pass datastructure sthat not necessarly are
        //defined with IEngine, but must be defined with IEngine implementations
        static void AddEngine<T>(T engine, Type[] entityViewTypes,
                              Dictionary<Type, FasterList<T>> engines) where T:IEngine
        {
            for (int i = 0; i < entityViewTypes.Length; i++)
            {
                var type = entityViewTypes[i];

                AddEngine(engine, engines, type);
            }
        }

        static void AddEngine<T>(T engine, Dictionary<Type, FasterList<T>> engines, Type type) where T : IEngine
        {
            FasterList<T> list;
            if (engines.TryGetValue(type, out list) == false)
            {
                list = new FasterList<T>();

                engines.Add(type, list);
            }

            list.Add(engine);
        }

        readonly Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> _entityEngines;    
        readonly FasterList<IEngine>                                             _otherEngines;
        
        static readonly Type _objectType = typeof(object);
    }
}