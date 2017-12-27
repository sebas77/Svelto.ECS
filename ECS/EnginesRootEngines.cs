using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;
using Svelto.Utilities;
using Svelto.WeakEvents;

#if EXPERIMENTAL
using Svelto.ECS.Experimental;
using Svelto.ECS.Experimental.Internal;
#endif

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
            _entityViewsDBdic = new Dictionary<Type, ITypeSafeDictionary>();
            
            _entityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>>();           
            _metaEntityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>>();
            _groupedEntityViewsToAdd = new DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeList>>>();

            _engineEntityViewDB = new EngineEntityViewDB(_entityViewsDB, _entityViewsDBdic, _metaEntityViewsDB, _groupEntityViewsDB);

            _scheduler = entityViewScheduler;
            _scheduler.Schedule(new WeakAction(SubmitEntityViews));
#if EXPERIMENTAL            
            _sharedStructEntityViewLists = new SharedStructEntityViewLists();
            _sharedGroupedStructEntityViewLists = new SharedGroupedStructEntityViewsLists();

            _structEntityViewEngineType = typeof(IStructEntityViewEngine<>);
            _groupedStructEntityViewsEngineType = typeof(IGroupedStructEntityViewsEngine<>);
            
            _implementedInterfaceTypes = new Dictionary<Type, Type[]>();
#endif
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
            var engineType = engine.GetType();
#if EXPERIMENTAL
            bool engineAdded;
    
            var implementedInterfaces = engineType.GetInterfaces();
            
            CollectImplementedInterfaces(implementedInterfaces);
    
            engineAdded = CheckSpecialEngine(engine);
#endif
            var viewEngine = engine as IHandleEntityViewEngine;
            
            if (viewEngine != null)
                CheckEntityViewsEngine(viewEngine, engineType);
            else            
                _otherEngines.Add(engine);
            
            var queryableEntityViewEngine = engine as IQueryingEntityViewEngine;
            if (queryableEntityViewEngine != null)
            {
                queryableEntityViewEngine.entityViewsDB = _engineEntityViewDB;
                queryableEntityViewEngine.Ready();
            }
        }
       
#if EXPERIMENTAL
         void CollectImplementedInterfaces(Type[] implementedInterfaces)
        {
            _implementedInterfaceTypes.Clear();

            var type = typeof(IHandleEntityViewEngine);

            for (int index = 0; index < implementedInterfaces.Length; index++)
            {
                var interfaceType = implementedInterfaces[index];

                if (type.IsAssignableFrom(interfaceType) == false)
                    continue;

                if (false == interfaceType.IsGenericTypeEx())
                {
                    continue;
                }

                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

                _implementedInterfaceTypes.Add(genericTypeDefinition, interfaceType.GetGenericArguments());
            }
        }
    
        bool CheckSpecialEngine(IEngine engine)
        {
            if (_implementedInterfaceTypes.Count == 0) return false;

            bool engineAdded = false;

            if (_implementedInterfaceTypes.ContainsKey(_structEntityViewEngineType))
            {
                ((IStructEntityViewEngine)engine).CreateStructEntityViews
                    (_sharedStructEntityViewLists);
            }

            if (_implementedInterfaceTypes.ContainsKey(_groupedStructEntityViewsEngineType))
            {
                ((IGroupedStructEntityViewsEngine)engine).CreateStructEntityViews
                    (_sharedGroupedStructEntityViewLists);
            }

            return engineAdded;
        }
#endif
        void CheckEntityViewsEngine(IEngine engine, Type engineType)
        {
            var baseType = engineType.GetBaseType();

            if (baseType.IsGenericTypeEx())
            {
                var genericArguments = baseType.GetGenericArgumentsEx();
                AddEngine(engine as IHandleEntityViewEngine, genericArguments, _entityViewEngines);
#if EXPERIMENTAL
                var activableEngine = engine as IHandleActivableEntityEngine;
                if (activableEngine != null)
                    AddEngine(activableEngine, genericArguments, _activableViewEntitiesEngines);
#endif    

                return;
            }

            throw new Exception("Not Supported Engine");
        }

        //The T parameter allows to pass datastructure sthat not necessarly are
        //defined with IEngine, but must be defined with IEngine implementations
        static void AddEngine<T>(T engine, Type[] types,
                              Dictionary<Type, FasterList<T>> engines) where T:IEngine
        {
            for (int i = 0; i < types.Length; i++)
            {
                FasterList<T> list;

                var type = types[i];

                if (engines.TryGetValue(type, out list) == false)
                {
                    list = new FasterList<T>();

                    engines.Add(type, list);
                }

                list.Add(engine);
            }
        }

        readonly Dictionary<Type, FasterList<IHandleEntityViewEngine>> _entityViewEngines;    

        readonly FasterList<IEngine> _otherEngines;

        readonly Dictionary<Type, ITypeSafeList> _entityViewsDB;
        readonly Dictionary<Type, ITypeSafeList> _metaEntityViewsDB;
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>> _groupEntityViewsDB;
        
        readonly Dictionary<Type, ITypeSafeDictionary> _entityViewsDBdic;

        readonly DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>> _entityViewsToAdd;
        readonly DoubleBufferedEntityViews<Dictionary<Type, ITypeSafeList>> _metaEntityViewsToAdd;
        readonly DoubleBufferedEntityViews<Dictionary<int, Dictionary<Type, ITypeSafeList>>> _groupedEntityViewsToAdd;
      
        readonly EntitySubmissionScheduler _scheduler;
#if EXPERIMENTAL
        readonly Type _structEntityViewEngineType;
        readonly Type _groupedStructEntityViewsEngineType;
        
        readonly SharedStructEntityViewLists _sharedStructEntityViewLists;
        readonly SharedGroupedStructEntityViewsLists _sharedGroupedStructEntityViewLists;

        readonly Dictionary<Type, Type[]>  _implementedInterfaceTypes;
#endif    

        readonly EngineEntityViewDB _engineEntityViewDB;

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