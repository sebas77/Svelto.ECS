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
            _entitiesOperations = new FasterDictionary<ulong, EntitySubmitOperation>();
            _reactiveEnginesAddRemove = new Dictionary<Type, FasterList<IEngine>>();
            _reactiveEnginesSwap = new Dictionary<Type, FasterList<IEngine>>();
            _enginesSet = new HashSet<IEngine>();
            _disposableEngines = new FasterList<IDisposable>();
            _transientEntitiesOperations = new FasterList<EntitySubmitOperation>();

            _groupEntityDB = new FasterDictionary<uint, Dictionary<Type, ITypeSafeDictionary>>();
            _groupsPerEntity = new Dictionary<Type, FasterDictionary<uint, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd();

            _entitiesStream = new EntitiesStream();
            _entitiesDB = new EntitiesDB(_groupEntityDB, _groupsPerEntity, _entitiesStream);
            
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
                if (engine is IReactOnAddAndRemove viewEngine)
                    CheckEntityViewsEngine<IReactOnAddAndRemove>(viewEngine, _reactiveEnginesAddRemove);
                
                if (engine is IReactOnSwap viewEngineSwap)
                    CheckEntityViewsEngine<IReactOnSwap>(viewEngineSwap, _reactiveEnginesSwap);

                _enginesSet.Add(engine);

                if (engine is IDisposable)
                    _disposableEngines.Add(engine as IDisposable);

                if (engine is IQueryingEntitiesEngine queryableEntityViewEngine)
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
       
        void CheckEntityViewsEngine<T>(IEngine engine, Dictionary<Type, FasterList<IEngine>> engines)
        {
            var interfaces = engine.GetType().GetInterfaces();

            foreach (var interf in interfaces)
            {
                if (interf.IsGenericTypeEx() && typeof(T).IsAssignableFrom(interf))
                {
                    var genericArguments = interf.GetGenericArgumentsEx();

                    AddEngine(engine, genericArguments, engines);
                }
            }
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
            if (engines.TryGetValue(type, out var list) == false)
            {
                list = new FasterList<T>();

                engines.Add(type, list);
            }

            list.Add(engine);
        }

        readonly Dictionary<Type, FasterList<IEngine>> _reactiveEnginesAddRemove;    
        readonly Dictionary<Type, FasterList<IEngine>> _reactiveEnginesSwap;
        readonly HashSet<IEngine>                      _enginesSet;
        readonly FasterList<IDisposable>               _disposableEngines;
        
        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks 
        //to the FasterDictionary capabilities OR it's possible to get a specific entityView indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        //ITypeSafeDictionary = Key = entityID, Value = EntityStruct
        readonly FasterDictionary<uint, Dictionary<Type, ITypeSafeDictionary>> _groupEntityDB;
        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id
                    //EntityViewType           //groupID  //entityID, EntityStruct
        readonly Dictionary<Type, FasterDictionary<uint, ITypeSafeDictionary>> _groupsPerEntity;
        
        readonly EntitiesStream _entitiesStream;
        readonly EntitiesDB     _entitiesDB;
        
        static readonly Type OBJECT_TYPE           = typeof(object);
        static readonly Type ENTITY_INFO_VIEW_TYPE = typeof(EntityStructInfoView);
    }
}