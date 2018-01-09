using System;
using System.Collections;
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
            _entityViewEngines = new Dictionary<Type, FasterList<IHandleEntityViewEngine>>();
            _otherEngines = new FasterList<IEngine>();

            _entityViewsDB = new Dictionary<Type, ITypeSafeList>();
            _metaEntityViewsDB = new Dictionary<Type, ITypeSafeList>();
            _groupEntityViewsDB = new Dictionary<int, Dictionary<Type, ITypeSafeList>>();
            _entityViewsDBDic = new Dictionary<Type, ITypeSafeDictionary>();
            
            _entityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>>();           
            _metaEntityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>>();
            _groupedEntityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeList>>>();

            _DB = new EntityViewsDB(_entityViewsDB, _entityViewsDBDic, _metaEntityViewsDB, _groupEntityViewsDB);

            _scheduler = entityViewScheduler;
            _scheduler.Schedule(new WeakAction(SubmitEntityViews));
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            UnityEngine.GameObject debugEngineObject = new UnityEngine.GameObject("Engine Debugger");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
#endif
        }

        public void AddEngine(IEngine engine)
        {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            Profiler.EngineProfiler.AddEngine(engine);
#endif
            var viewEngine = engine as IHandleEntityViewEngine;
            
            if (viewEngine != null)
                CheckEntityViewsEngine(viewEngine);
            else            
                _otherEngines.Add(engine);
            
            var queryableEntityViewEngine = engine as IQueryingEntityViewEngine;
            if (queryableEntityViewEngine != null)
            {
                queryableEntityViewEngine.entityViewsDB = _DB;
                queryableEntityViewEngine.Ready();
            }
        }
       
        void CheckEntityViewsEngine(IEngine engine)
        {
            var baseType = engine.GetType().GetBaseType();

            while (baseType != _object)
            {
                if (baseType.IsGenericTypeEx())
                {
                    var genericArguments = baseType.GetGenericArgumentsEx();
                    AddEngine(engine as IHandleEntityViewEngine, genericArguments, _entityViewEngines);

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

        readonly Dictionary<Type, FasterList<IHandleEntityViewEngine>> _entityViewEngines;    
        readonly FasterList<IEngine> _otherEngines;
        
        static readonly Type _entityViewType= typeof(EntityView);
        static readonly Type _object = typeof(object);

        class DoubleBufferedEntityViews<T> where T : class, IDictionary, new()
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();

            internal DoubleBufferedEntityViews()
            {
                this.other = _entityViewsToAddBufferA;
                this.current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;

            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }
        }
    }
}