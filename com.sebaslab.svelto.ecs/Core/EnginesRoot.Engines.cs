#if PROFILE_SVELTO && DEBUG
#warning the global define PROFILE_SVELTO should be used only when it's necessary to profile in order to reduce the overhead of debug code. Normally remove this define to get insights when errors happen
#endif

using System;
using System.Collections.Generic;
using DBC.ECS;
using Svelto.Common;
using Svelto.Common.DataStructures;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        static EnginesRoot()
        {
            GroupHashMap.Init();
            SharedDictonary.Init();
            SerializationDescriptorMap.Init();

            _swapEntities   = SwapEntities;
            _removeEntities = RemoveEntities;
            _removeGroup    = RemoveGroup;
            _swapGroup      = SwapGroup;
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
            _entitiesOperations                  = new EntitiesOperations();
            _idChecker                           = new FasterDictionary<ExclusiveGroupStruct, HashSet<uint>>();

            _cachedRangeOfSubmittedIndices        = new FasterList<(uint, uint)>();
            _transientEntityIDsLeftAndAffectedByRemoval    = new FasterList<uint>();
            _transientEntityIDsLeftWithoutDuplicates = new FasterDictionary<uint, int>();
            
            _multipleOperationOnSameEGIDChecker            = new FasterDictionary<EGID, uint>();
#if UNITY_NATIVE //because of the thread count, ATM this is only for unity
            _nativeSwapOperationQueue   = new Svelto.ECS.DataStructures.AtomicNativeBags(Allocator.Persistent);
            _nativeRemoveOperationQueue = new Svelto.ECS.DataStructures.AtomicNativeBags(Allocator.Persistent);
            _nativeAddOperationQueue    = new Svelto.ECS.DataStructures.AtomicNativeBags(Allocator.Persistent);
#endif
            _serializationDescriptorMap = new SerializationDescriptorMap();
            _reactiveEnginesAdd = new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>>();
            _reactiveEnginesAddEx =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>>();
            _reactiveEnginesRemove =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>>();
            _reactiveEnginesRemoveEx =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>>();
            _reactiveEnginesSwap =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>>();
            _reactiveEnginesSwapEx =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>>();
            _reactiveEnginesDispose =
                new FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>>();

            _reactiveEnginesSubmission = new FasterList<IReactOnSubmission>();
            _enginesSet                = new FasterList<IEngine>();
            _enginesTypeSet            = new HashSet<Type>();
            _disposableEngines         = new FasterList<IDisposable>();

            _groupEntityComponentsDB =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();
            _groupsPerEntity =
                new FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd();
            _entityStreams      = EntitiesStreams.Create();
            _groupFilters =
                new FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>>();
            _entityLocator.InitEntityReferenceMap();
            _entitiesDB = new EntitiesDB(this, _entityLocator);

            InitFilters();

            scheduler        = entitiesComponentScheduler;
            scheduler.onTick = new EntitiesSubmitter(this);
#if UNITY_NATIVE
            AllocateNativeOperations();
#endif
        }

        protected EnginesRoot(EntitiesSubmissionScheduler entitiesComponentScheduler,
            EnginesReadyOption enginesWaitForReady) : this(entitiesComponentScheduler)
        {
            _enginesWaitForReady = enginesWaitForReady;
        }

        public EntitiesSubmissionScheduler scheduler { get; }

        /// <summary>
        ///     Dispose an EngineRoot once not used anymore, so that all the
        ///     engines are notified with the entities removed.
        ///     It's a clean up process.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void AddEngine(IEngine engine)
        {
            var type       = engine.GetType();
            var refWrapper = new RefWrapperType(type);
            Check.Require(engine != null, "Engine to add is invalid or null");
            Check.Require(
                _enginesTypeSet.Contains(refWrapper) == false ||
                type.ContainsCustomAttribute(typeof(AllowMultipleAttribute)),
                "The same engine has been added more than once, if intentional, use [AllowMultiple] class attribute "
                   .FastConcat(engine.ToString()));
            try
            {
                if (engine is IReactOnAdd viewEngineAdd)
                    CheckReactEngineComponents(typeof(IReactOnAdd<>), viewEngineAdd, _reactiveEnginesAdd, type.Name);

                if (engine is IReactOnAddEx viewEngineAddEx)
                    CheckReactEngineComponents(typeof(IReactOnAddEx<>), viewEngineAddEx, _reactiveEnginesAddEx, type.Name);

                if (engine is IReactOnRemove viewEngineRemove)
                    CheckReactEngineComponents(typeof(IReactOnRemove<>), viewEngineRemove, _reactiveEnginesRemove, type.Name);

                if (engine is IReactOnRemoveEx viewEngineRemoveEx)
                    CheckReactEngineComponents(typeof(IReactOnRemoveEx<>), viewEngineRemoveEx, _reactiveEnginesRemoveEx, type.Name);

                if (engine is IReactOnDispose viewEngineDispose)
                    CheckReactEngineComponents(typeof(IReactOnDispose<>), viewEngineDispose, _reactiveEnginesDispose, type.Name);

                if (engine is IReactOnSwap viewEngineSwap)
                    CheckReactEngineComponents(typeof(IReactOnSwap<>), viewEngineSwap, _reactiveEnginesSwap, type.Name);

                if (engine is IReactOnSwapEx viewEngineSwapEx)
                    CheckReactEngineComponents(typeof(IReactOnSwapEx<>), viewEngineSwapEx, _reactiveEnginesSwapEx, type.Name);

                if (engine is IReactOnSubmission submissionEngine)
                    _reactiveEnginesSubmission.Add(submissionEngine);

                _enginesTypeSet.Add(refWrapper);
                _enginesSet.Add(engine);

                if (engine is IDisposable)
                    _disposableEngines.Add(engine as IDisposable);

                if (engine is IQueryingEntitiesEngine queryableEntityComponentEngine)
                    queryableEntityComponentEngine.entitiesDB = _entitiesDB;

                if (_enginesWaitForReady == EnginesReadyOption.ReadyAsAdded && engine is IGetReadyEngine getReadyEngine)
                    getReadyEngine.Ready();
            }
            catch (Exception e)
            {
                throw new ECSException("Code crashed while adding engine ".FastConcat(engine.GetType().ToString(), " "),
                    e);
            }
        }

        public void Ready()
        {
            Check.Require(_enginesWaitForReady == EnginesReadyOption.WaitForReady,
                "The engine has not been initialise to wait for an external ready trigger");

            foreach (var engine in _enginesSet)
                if (engine is IGetReadyEngine getReadyEngine)
                    getReadyEngine.Ready();
        }

        static void AddEngineToList<T>(T engine, Type[] entityComponentTypes,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<T>>> engines, string typeName)
            where T : class, IReactEngine
        {
            for (var i = 0; i < entityComponentTypes.Length; i++)
            {
                var type = entityComponentTypes[i];

                if (engines.TryGetValue(new RefWrapperType(type), out var list) == false)
                {
                    list = new FasterList<ReactEngineContainer<T>>();

                    engines.Add(new RefWrapperType(type), list);
                }

                list.Add(new ReactEngineContainer<T>(engine, typeName));
            }
        }

        void CheckReactEngineComponents<T>(Type genericDefinition, T engine,
            FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<T>>> engines, string typeName)
            where T : class, IReactEngine
        {
            var interfaces = engine.GetType().GetInterfaces();

            foreach (var interf in interfaces)
            {
                if (interf.IsGenericTypeEx() && interf.GetGenericTypeDefinition() == genericDefinition)
                {
                    var genericArguments = interf.GetGenericArgumentsEx();

                    AddEngineToList(engine, genericArguments, engines, typeName);
                }
            }
        }

        void Dispose(bool disposing)
        {
            _isDisposing = disposing;

            if (disposing == false)
                return;

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
                foreach (var entityList in groups.value)
                    try
                    {
                        ITypeSafeDictionary typeSafeDictionary = entityList.value;
                        
                        typeSafeDictionary.ExecuteEnginesDisposeCallbacks_Group(_reactiveEnginesDispose, groups.key,
                            profiler);
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e);
                    }

                foreach (var groups in _groupEntityComponentsDB)
                foreach (var entityList in groups.value)
                    entityList.value.Dispose();

                foreach (var type in _groupFilters)
                foreach (var group in type.value)
                    group.value.Dispose();

                _groupFilters.Clear();

                DisposeFilters();

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
                _reactiveEnginesAdd.Clear();
                _reactiveEnginesRemove.Clear();
                _reactiveEnginesDispose.Clear();
                _reactiveEnginesSubmission.Clear();

                _groupedEntityToAdd.Dispose();

                _entityLocator.DisposeEntityReferenceMap();

                _entityStreams.Dispose();
                scheduler.Dispose();
            }
        }

        void NotifyReactiveEnginesOnSubmission()
        {
            var enginesCount = _reactiveEnginesSubmission.count;
            for (var i = 0; i < enginesCount; i++)
                _reactiveEnginesSubmission[i].EntitiesSubmitted();
        }

        public readonly struct EntitiesSubmitter
        {
            public EntitiesSubmitter(EnginesRoot enginesRoot) : this()
            {
                _enginesRoot = new Svelto.DataStructures.WeakReference<EnginesRoot>(enginesRoot);
            }

            internal void SubmitEntities()
            {
                Check.Require(_enginesRoot.IsValid, "ticking an GCed engines root?");

                var enginesRootTarget           = _enginesRoot.Target;
                var entitiesSubmissionScheduler = enginesRootTarget.scheduler;

                if (entitiesSubmissionScheduler.paused == false)
                {
                    Check.Require(entitiesSubmissionScheduler.isRunning == false,
                        "A submission started while the previous one was still flushing");
                    entitiesSubmissionScheduler.isRunning = true;

                    using (var profiler = new PlatformProfiler("Svelto.ECS - Entities Submission"))
                    {
                        var iterations       = 0;
                        var hasEverSubmitted = false;

                        // We need to clear transient filters before processing callbacks since the callbacks may add
                        // new entities to these filters.
                        enginesRootTarget.ClearTransientFilters();

#if UNITY_NATIVE
                        enginesRootTarget.FlushNativeOperations(profiler);
#endif
                        //todo: proper unit test structural changes made as result of add/remove callbacks
                        while (enginesRootTarget.HasMadeNewStructuralChangesInThisIteration() 
                               && iterations++ < MAX_SUBMISSION_ITERATIONS)
                        {
                            hasEverSubmitted = true;

                            _enginesRoot.Target.SingleSubmission(profiler);
#if UNITY_NATIVE
                            if (enginesRootTarget.HasMadeNewStructuralChangesInThisIteration())
                                enginesRootTarget.FlushNativeOperations(profiler);
#endif
                        }

#if DEBUG && !PROFILE_SVELTO
                        if (iterations == MAX_SUBMISSION_ITERATIONS)
                            throw new ECSException("possible circular submission detected");
#endif
                        if (hasEverSubmitted)
                            enginesRootTarget.NotifyReactiveEnginesOnSubmission();
                    }

                    entitiesSubmissionScheduler.isRunning = false;
                    ++entitiesSubmissionScheduler.iteration;
                }
            }

            readonly Svelto.DataStructures.WeakReference<EnginesRoot> _enginesRoot;
        }

        ~EnginesRoot()
        {
            Console.LogWarning("Engines Root has been garbage collected, don't forget to call Dispose()!");

            Dispose(false);
        }

        const int MAX_SUBMISSION_ITERATIONS = 10;

        internal bool                    _isDisposing;
        readonly FasterList<IDisposable> _disposableEngines;
        readonly FasterList<IEngine>     _enginesSet;
        readonly HashSet<Type>           _enginesTypeSet;
        readonly EnginesReadyOption      _enginesWaitForReady;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> _reactiveEnginesAdd;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>>
            _reactiveEnginesAddEx;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>>
            _reactiveEnginesRemove;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>>
            _reactiveEnginesRemoveEx;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> _reactiveEnginesSwap;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>>
            _reactiveEnginesSwapEx;

        readonly FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>>
            _reactiveEnginesDispose;

        readonly FasterList<IReactOnSubmission> _reactiveEnginesSubmission;
    }

    public enum EnginesReadyOption
    {
        ReadyAsAdded,
        WaitForReady
    }
}