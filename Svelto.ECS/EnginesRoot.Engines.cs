using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
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
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR        
        static EnginesRoot()
        {

/// <summary>
/// I still need to find a good solution for this. Need to move somewhere else
/// </summary>
            UnityEngine.GameObject debugEngineObject = new UnityEngine.GameObject("Svelto.ECS.Profiler");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
            UnityEngine.GameObject.DontDestroyOnLoad(debugEngineObject);
        }
#endif    
       
        /// <summary>
        /// Engines root contextualize your engines and entities. You don't need to limit yourself to one EngineRoot
        /// as multiple engines root could promote separation of scopes. The EntitySubmissionScheduler checks
        /// periodically if new entity must be submitted to the database and the engines. It's an external
        /// dependencies to be independent by the running platform as the user can define it.
        /// The EntitySubmissionScheduler cannot hold an EnginesRoot reference, that's why
        /// it must receive a weak reference of the EnginesRoot callback.
        /// </summary>
        public EnginesRoot(IEntitySubmissionScheduler entityViewScheduler)
        {
            _entitiesOperations = new FasterList<EntitySubmitOperation>();
            _entityEngines = new Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>();
            _otherEngines = new FasterList<IEngine>();
            _disposableEngines = new FasterList<IDisposable>();
            _transientEntitiesOperations = new FasterList<EntitySubmitOperation>();

            _groupEntityDB = new FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>();
            _groupsPerEntity = new Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd<FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>>();

            _DB = new EntitiesDB(_groupEntityDB, _groupsPerEntity);

            _scheduler = entityViewScheduler;
            _scheduler.onTick = new WeakAction(SubmitEntityViews);
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
            
            if (engine is IDisposable)
                _disposableEngines.Add(engine as IDisposable);
            
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
        readonly FasterList<IDisposable>                                         _disposableEngines;
        
        static readonly Type _objectType = typeof(object);
    }
}