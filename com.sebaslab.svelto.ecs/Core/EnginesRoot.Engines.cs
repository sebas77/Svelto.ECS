using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public sealed partial class EnginesRoot
    {
        static EnginesRoot()
        {
            GroupHashMap.Init();
            SerializationDescriptorMap.Init();
        }
        /// <summary>
        ///     Engines root contextualize your engines and entities. You don't need to limit yourself to one EngineRoot
        ///     as multiple engines root could promote separation of scopes. The EntitySubmissionScheduler checks
        ///     periodically if new entity must be submitted to the database and the engines. It's an external
        ///     dependencies to be independent by the running platform as the user can define it.
        ///     The EntitySubmissionScheduler cannot hold an EnginesRoot reference, that's why
        ///     it must receive a weak reference of the EnginesRoot callback.
        /// </summary>
        public EnginesRoot(EntitiesSubmissionScheduler entitiesComponentScheduler)
        {
            _entitiesOperations            = new FasterDictionary<ulong, EntitySubmitOperation>();
#if UNITY_NATIVE //because of the thread count, ATM this is only for unity            
            _nativeSwapOperationQueue   = new DataStructures.AtomicNativeBags(Allocator.Persistent);
            _nativeRemoveOperationQueue = new DataStructures.AtomicNativeBags(Allocator.Persistent);
            _nativeAddOperationQueue    = new DataStructures.AtomicNativeBags(Allocator.Persistent);
#endif            
            _serializationDescriptorMap     = new SerializationDescriptorMap();
            _maxNumberOfOperationsPerFrame = uint.MaxValue;
            _reactiveEnginesAddRemove      = new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>>();
            _reactiveEnginesAddRemoveOnDispose =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>>();
            _reactiveEnginesSwap         = new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>>();
            _reactiveEnginesSubmission   = new FasterList<IReactOnSubmission>();
            _enginesSet                  = new FasterList<IEngine>();
            _enginesTypeSet              = new HashSet<Type>();
            _disposableEngines           = new FasterList<IDisposable>();
            _transientEntitiesOperations = new FasterList<EntitySubmitOperation>();

            _groupEntityComponentsDB =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();
            _groupsPerEntity =
                new FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd();
            _entityStreams = EntitiesStreams.Create();
            _groupFilters =
                new FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>>();
            _entityLocator.InitEntityReferenceMap();
            _entitiesDB = new EntitiesDB(this,_entityLocator);

            scheduler        = entitiesComponentScheduler;
            scheduler.onTick = new EntitiesSubmitter(this);
#if UNITY_NATIVE
            AllocateNativeOperations();
#endif
        }

        public EnginesRoot
            (EntitiesSubmissionScheduler entitiesComponentScheduler, bool isDeserializationOnly) :
            this(entitiesComponentScheduler)
        {
            _isDeserializationOnly = isDeserializationOnly;
        }

        public EntitiesSubmissionScheduler scheduler { get; }

        /// <summary>
        ///     Dispose an EngineRoot once not used anymore, so that all the
        ///     engines are notified with the entities removed.
        ///     It's a clean up process.
        /// </summary>
        public void Dispose()
        {
            _isDisposing = true;

            using (var profiler = new PlatformProfiler("Final Dispose"))
            {
                //Note: The engines are disposed before the the remove callback to give the chance to behave
                //differently if a remove happens as a consequence of a dispose
                //The pattern is to implement the IDisposable interface and set a flag in the engine. The
                //remove callback will then behave differently according the flag.
                foreach (var engine in _disposableEngines)
                    try
                    {
                        if (engine is IDisposingEngine dengine)
                            dengine.isDisposing = true;
                        engine.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e);
                    }

                foreach (var groups in _groupEntityComponentsDB)
                foreach (var entityList in groups.Value)
                    try
                    {
                        entityList.Value.ExecuteEnginesRemoveCallbacks(_reactiveEnginesAddRemoveOnDispose, profiler
                                                                     , new ExclusiveGroupStruct(groups.Key));
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e);
                    }

                foreach (var groups in _groupEntityComponentsDB)
                foreach (var entityList in groups.Value)
                    entityList.Value.Dispose();

                foreach (var type in _groupFilters)
                foreach (var group in type.Value)
                    group.Value.Dispose();

                _groupFilters.Clear();

#if UNITY_NATIVE
                _nativeAddOperationQueue.Dispose();
                _nativeRemoveOperationQueue.Dispose();
                _nativeSwapOperationQueue.Dispose();
#endif
                _groupEntityComponentsDB.Clear();
                _groupsPerEntity.Clear();

                _disposableEngines.Clear();
                _enginesSet.Clear();
                _enginesTypeSet.Clear();
                _reactiveEnginesSwap.Clear();
                _reactiveEnginesAddRemove.Clear();
                _reactiveEnginesAddRemoveOnDispose.Clear();
                _reactiveEnginesSubmission.Clear();

                _entitiesOperations.Clear();
                _transientEntitiesOperations.Clear();

                _groupedEntityToAdd.Dispose();

                _entityLocator.DisposeEntityReferenceMap();
                
                _entityStreams.Dispose();
                scheduler.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public void AddEngine(IEngine engine)
        {
            var type       = engine.GetType();
            var refWrapper = new RefWrapperType(type);
            DBC.ECS.Check.Require(engine != null, "Engine to add is invalid or null");
            DBC.ECS.Check.Require(
                _enginesTypeSet.Contains(refWrapper) == false
             || type.ContainsCustomAttribute(typeof(AllowMultipleAttribute)) == true
              , "The same engine has been added more than once, if intentional, use [AllowMultiple] class attribute "
                   .FastConcat(engine.ToString()));
            try
            {
                if (engine is IReactOnAddAndRemove viewEngine)
                    CheckReactEngineComponents(viewEngine, _reactiveEnginesAddRemove, type.Name);

                if (engine is IReactOnDispose viewEngineDispose)
                    CheckReactEngineComponents(viewEngineDispose, _reactiveEnginesAddRemoveOnDispose, type.Name);

                if (engine is IReactOnSwap viewEngineSwap)
                    CheckReactEngineComponents(viewEngineSwap, _reactiveEnginesSwap, type.Name);

                if (engine is IReactOnSubmission submissionEngine)
                    _reactiveEnginesSubmission.Add(submissionEngine);

                _enginesTypeSet.Add(refWrapper);
                _enginesSet.Add(engine);

                if (engine is IDisposable)
                    _disposableEngines.Add(engine as IDisposable);

                if (engine is IQueryingEntitiesEngine queryableEntityComponentEngine)
                {
                    queryableEntityComponentEngine.entitiesDB = _entitiesDB;
                    queryableEntityComponentEngine.Ready();
                }
            }
            catch (Exception e)
            {
                throw new ECSException("Code crashed while adding engine ".FastConcat(engine.GetType().ToString(), " ")
                                     , e);
            }
        }

        void NotifyReactiveEnginesOnSubmission()
        {
            var enginesCount = _reactiveEnginesSubmission.count;
            for (var i = 0; i < enginesCount; i++)
                _reactiveEnginesSubmission[i].EntitiesSubmitted();
        }

        ~EnginesRoot()
        {
            Console.LogWarning("Engines Root has been garbage collected, don't forget to call Dispose()!");

            Dispose();
        }

        void CheckReactEngineComponents<T>
            (T engine, FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> engines, string typeName)
            where T : class, IReactEngine
        {
            var interfaces = engine.GetType().GetInterfaces();

            foreach (var interf in interfaces)
                if (interf.IsGenericTypeEx() && typeof(T).IsAssignableFrom(interf))
                {
                    var genericArguments = interf.GetGenericArgumentsEx();

                    AddEngineToList(engine, genericArguments, engines, typeName);
                }
        }

        static void AddEngineToList<T>
        (T engine, Type[] entityComponentTypes
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> engines, string typeName)
            where T : class, IReactEngine
        {
            for (var i = 0; i < entityComponentTypes.Length; i++)
            {
                var type = entityComponentTypes[i];

                if (engines.TryGetValue(new RefWrapperType(type), out var list) == false)
                {
                    list = new FasterList<ReactEngineContainer>();

                    engines.Add(new RefWrapperType(type), list);
                }

                list.Add(new ReactEngineContainer(engine, typeName));
            }
        }

        internal bool                    _isDisposing;
        readonly FasterList<IDisposable> _disposableEngines;
        readonly FasterList<IEngine>     _enginesSet;
        readonly HashSet<Type>           _enginesTypeSet;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> _reactiveEnginesAddRemove;
        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> _reactiveEnginesAddRemoveOnDispose;
        readonly FasterList<IReactOnSubmission>                                     _reactiveEnginesSubmission;
        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer>> _reactiveEnginesSwap;

        public struct EntitiesSubmitter
        {
            public EntitiesSubmitter(EnginesRoot enginesRoot) : this()
            {
                _enginesRoot = new Svelto.DataStructures.WeakReference<EnginesRoot>(enginesRoot);
                _privateSubmitEntities =
                    _enginesRoot.Target.SingleSubmission(new PlatformProfiler());
                submitEntities = Invoke(); //this must be last to capture all the variables
            }

            IEnumerator<bool> Invoke()
            {
                while (true)
                {
                    DBC.ECS.Check.Require(_enginesRoot.IsValid, "ticking an GCed engines root?");

                    var enginesRootTarget           = _enginesRoot.Target;
                    var entitiesSubmissionScheduler = enginesRootTarget.scheduler;

                    if (entitiesSubmissionScheduler.paused == false)
                    {
                        DBC.ECS.Check.Require(entitiesSubmissionScheduler.isRunning == false
                                            , "A submission started while the previous one was still flushing");
                        entitiesSubmissionScheduler.isRunning = true;

                        using (var profiler = new PlatformProfiler("Svelto.ECS - Entities Submission"))
                        {
                            var iterations       = 0;
                            var hasEverSubmitted = false;
#if UNITY_NATIVE
                            enginesRootTarget.FlushNativeOperations(profiler);
#endif

                            //todo: proper unit test structural changes made as result of add/remove callbacks
                            while (enginesRootTarget.HasMadeNewStructuralChangesInThisIteration() && iterations++ < 5)
                            {
                                hasEverSubmitted = true;

                                while (true)
                                {
                                    _privateSubmitEntities.MoveNext();
                                    if (_privateSubmitEntities.Current == true)
                                    {
                                        using (profiler.Yield())
                                        {
                                            yield return true;
                                        }
                                    }
                                    else
                                        break;
                                }
#if UNITY_NATIVE
                                if (enginesRootTarget.HasMadeNewStructuralChangesInThisIteration())
                                    enginesRootTarget.FlushNativeOperations(profiler);
#endif
                            }

#if DEBUG && !PROFILE_SVELTO
                            if (iterations == 5)
                                throw new ECSException("possible circular submission detected");
#endif
                            if (hasEverSubmitted)
                                enginesRootTarget.NotifyReactiveEnginesOnSubmission();
                        }

                        entitiesSubmissionScheduler.isRunning = false;
                        ++entitiesSubmissionScheduler.iteration;
                    }

                    yield return false;
                }
            }

            public uint maxNumberOfOperationsPerFrame
            {
                set => _enginesRoot.Target._maxNumberOfOperationsPerFrame = value;
            }

            readonly Svelto.DataStructures.WeakReference<EnginesRoot> _enginesRoot;

            internal readonly IEnumerator<bool> submitEntities;
            readonly          IEnumerator<bool> _privateSubmitEntities;
        }
    }
}