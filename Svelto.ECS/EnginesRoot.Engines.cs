using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
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
#if DEBUG && !PROFILER            
            _entitiesOperationsDebug = new FasterDictionary<long, EntitySubmitOperationType>();
#endif            
            _entitiesOperations = new FasterList<EntitySubmitOperation>();
            _entityEngines = new Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>();
            _enginesSet = new HashSet<IEngine>();
            _disposableEngines = new FasterList<IDisposable>();
            _transientEntitiesOperations = new FasterList<EntitySubmitOperation>();

            _groupEntityDB = new FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>();
            _groupsPerEntity = new Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd<FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>>>();

            _entitiesDB = new EntitiesDB(_groupEntityDB, _groupsPerEntity);
            _entitiesStream = new EntitiesStream(_entitiesDB);

            _scheduler = entityViewScheduler;
            _scheduler.onTick = new WeakAction(SubmitEntityViews);
        }

        public void AddEngine(IEngine engine)
        {
            DBC.ECS.Check.Require(_enginesSet.Contains(engine) == false,
                                 "The same engine has been added more than once "
                                    .FastConcat(engine.ToString()));

            try
            {
                var viewEngine = engine as IHandleEntityViewEngineAbstracted;

                if (viewEngine != null)
                    CheckEntityViewsEngine(viewEngine);

                _enginesSet.Add(engine);

                if (engine is IDisposable)
                    _disposableEngines.Add(engine as IDisposable);

                var queryableEntityViewEngine = engine as IQueryingEntitiesEngine;
                if (queryableEntityViewEngine != null)
                {
                    queryableEntityViewEngine.entitiesDB = _entitiesDB;
                    queryableEntityViewEngine.Ready();
                }
            }
            catch (Exception e)
            {
#if !DEBUG                
                throw new ECSException("Code crashed while adding engine ".FastConcat(engine.GetType().ToString()), e);
#else
                Console.LogException("Code crashed while adding engine ".FastConcat(engine.GetType().ToString()), e);
#endif                
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

            throw new ArgumentException("Not Supported Engine " + engine);
        }

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
        readonly HashSet<IEngine>                                                _enginesSet;
        readonly FasterList<IDisposable>                                         _disposableEngines;
        
        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks 
        //to the FasterDictionary capabilities OR it's possible to get a specific entityView indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        readonly FasterDictionary<int, Dictionary<Type, ITypeSafeDictionary>> _groupEntityDB;
        readonly EntitiesDB                                                   _entitiesDB;
        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id 
        readonly Dictionary<Type, FasterDictionary<int, ITypeSafeDictionary>> _groupsPerEntity; //yes I am being sarcastic
        
        static readonly Type _objectType = typeof(object);
    }
}