using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        public struct EntitiesSubmitter
        {
            public EntitiesSubmitter(EnginesRoot enginesRoot)
            {
                _weakReference = new DataStructures.WeakReference<EnginesRoot>(enginesRoot);
            }

            public void Invoke()
            {
                if (_weakReference.IsValid)
                    _weakReference.Target.SubmitEntityViews();
            }

            readonly DataStructures.WeakReference<EnginesRoot> _weakReference;
        }
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
            serializationDescriptorMap = new SerializationDescriptorMap();
            _reactiveEnginesAddRemove = new FasterDictionary<RefWrapper<Type>, FasterList<IEngine>>();
            _reactiveEnginesSwap = new FasterDictionary<RefWrapper<Type>, FasterList<IEngine>>();
            _enginesSet = new FasterList<IEngine>();
            _enginesTypeSet = new HashSet<Type>();
            _disposableEngines = new FasterList<IDisposable>();
            _transientEntitiesOperations = new FasterList<EntitySubmitOperation>();

            _groupEntityViewsDB = new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();
            _groupsPerEntity = new FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd();

            _entitiesStream = new EntitiesStream();
            _entitiesDB = new EntitiesDB(_groupEntityViewsDB, _groupsPerEntity, _entitiesStream);

            _scheduler = entityViewScheduler;
            _scheduler.onTick = new EntitiesSubmitter(this);
        }
        
        public EnginesRoot(IEntitySubmissionScheduler entityViewScheduler, bool isDeserializationOnly):this(entityViewScheduler)
        {
            _isDeserializationOnly = isDeserializationOnly;
        }

        public void AddEngine(IEngine engine)
        {
            var type = engine.GetType();
            var refWrapper = new RefWrapper<Type>(type);
            DBC.ECS.Check.Require(
                _enginesTypeSet.Contains(refWrapper) == false ||
                type.ContainsCustomAttribute(typeof(AllowMultipleAttribute)) == true,
                "The same engine has been added more than once, if intentional, use [AllowMultiple] class attribute "
                    .FastConcat(engine.ToString()));
            try
            {
                if (engine is IReactOnAddAndRemove viewEngine)
                    CheckEntityViewsEngine(viewEngine, _reactiveEnginesAddRemove);

                if (engine is IReactOnSwap viewEngineSwap)
                    CheckEntityViewsEngine(viewEngineSwap, _reactiveEnginesSwap);

                _enginesTypeSet.Add(refWrapper);
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
                throw new ECSException("Code crashed while adding engine ".FastConcat(engine.GetType().ToString(), " "), e);
            }
        }

        void CheckEntityViewsEngine<T>(T engine, FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines)
            where T : class, IEngine
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
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines) 
            where T : class, IEngine
        {
            for (var i = 0; i < entityViewTypes.Length; i++)
            {
                var type = entityViewTypes[i];

                AddEngine(engine, engines, type);
            }
        }

        static void AddEngine<T>(T engine, FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, Type type)
            where T : class, IEngine
        {
            if (engines.TryGetValue(new RefWrapper<Type>(type), out var list) == false)
            {
                list = new FasterList<IEngine>();

                engines.Add(new RefWrapper<Type>(type), list);
            }

            list.Add(engine);
        }

        readonly FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> _reactiveEnginesAddRemove;
        readonly FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> _reactiveEnginesSwap;
        readonly FasterList<IDisposable>                                 _disposableEngines;
        
        readonly FasterList<IEngine> _enginesSet;
        readonly HashSet<Type>       _enginesTypeSet;
    }
}