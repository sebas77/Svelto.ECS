#if PROFILE_SVELTO && DEBUG
#warning the global define PROFILE_SVELTO should be used only when it's necessary to profile in order to reduce the overhead of debug code. While debugging remove this define to get insights when errors happen
#endif

using System;
using System.Collections.Generic;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        static EnginesRoot()
        {
            EntityDescriptorsWarmup.WarmUp();
            GroupHashMap.WarmUp();
            SerializationDescriptorMap.Init();

            _swapEntities = SwapEntities;
            _removeEntities = RemoveEntities;
            _removeGroup = RemoveGroup;
            _swapGroup = SwapGroup;
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
            _entitiesOperations = new EntitiesOperations();

            _cachedRangeOfSubmittedIndices = new FasterList<(uint, uint)>();
            _transientEntityIDsAffectedByRemoveAtSwapBack = new FasterDictionary<uint, uint>();
            
            InitDebugChecks();
#if UNITY_NATIVE //because of the thread count, ATM this is only for unity
            _nativeSwapOperationQueue = new AtomicNativeBags(Allocator.Persistent);
            _nativeRemoveOperationQueue = new AtomicNativeBags(Allocator.Persistent);
            _nativeAddOperationQueue = new AtomicNativeBags(Allocator.Persistent);
#endif
            _serializationDescriptorMap = new SerializationDescriptorMap();
            _reactiveEnginesAdd = new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>>();
            _reactiveEnginesAddEx =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>>();
            _reactiveEnginesRemove =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>>();
            _reactiveEnginesRemoveEx =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>>();
            _reactiveEnginesSwap =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>>();
            _reactiveEnginesSwapEx =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>>();
            _reactiveEnginesDispose =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>>();
            _reactiveEnginesDisposeEx =
                new FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>>();

            _reactiveEnginesSubmission = new FasterList<IReactOnSubmission>();
            _reactiveEnginesSubmissionStarted = new FasterList<IReactOnSubmissionStarted>();
            _enginesSet = new FasterList<IEngine>();
            _enginesTypeSet = new HashSet<Type>();
            _disposableEngines = new FasterList<IDisposable>();

            _groupEntityComponentsDB =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>();
            _groupsPerEntity =
                new FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>();
            _groupedEntityToAdd = new DoubleBufferedEntitiesToAdd();
            _entityStreams = EntitiesStreams.Create();

            _entityLocator.InitEntityReferenceMap();
            _entitiesDB = new EntitiesDB(this, _entityLocator);

            InitFilters();

            scheduler = entitiesComponentScheduler;
            scheduler.onTick = new EntitiesSubmitter(this);
#if UNITY_NATIVE
            AllocateNativeOperations();
#endif
        }

        /// <summary>
        ///Ready is a callback that can be used to signal that an engine is ready to be used because the entitiesDB is now available
        ///usually engines are ready to be used when they are added to the enginesRoot, but in some special cases, it is possible to
        ///wait for the user input to signal that engines are ready to be used
        /// </summary>
        protected EnginesRoot(EntitiesSubmissionScheduler entitiesComponentScheduler,
            EnginesReadyOption enginesWaitForReady): this(entitiesComponentScheduler)
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

        public bool IsValid()
        {
            return _isDisposed == false;
        }
        
        public void AddEngine(IEngine engine, bool addSubEngines = true)
        {
            var type = engine.GetType();
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
#pragma warning disable CS0612
                    CheckReactEngineComponents(typeof(IReactOnAdd<>), viewEngineAdd, _reactiveEnginesAdd, type.Name);
#pragma warning restore CS0612

                if (engine is IReactOnAddEx viewEngineAddEx)
                    CheckReactEngineComponents(
                        typeof(IReactOnAddEx<>), viewEngineAddEx, _reactiveEnginesAddEx, type.Name);

                if (engine is IReactOnRemove viewEngineRemove)
                    CheckReactEngineComponents(
#pragma warning disable CS0612
                        typeof(IReactOnRemove<>), viewEngineRemove, _reactiveEnginesRemove, type.Name);
#pragma warning restore CS0612

                if (engine is IReactOnRemoveEx viewEngineRemoveEx)
                    CheckReactEngineComponents(
                        typeof(IReactOnRemoveEx<>), viewEngineRemoveEx, _reactiveEnginesRemoveEx, type.Name);

                if (engine is IReactOnDispose viewEngineDispose)
                    CheckReactEngineComponents(
#pragma warning disable CS0618
                        typeof(IReactOnDispose<>), viewEngineDispose, _reactiveEnginesDispose, type.Name);
#pragma warning restore CS0618
                
                if (engine is IReactOnDisposeEx viewEngineDisposeEx)
                    CheckReactEngineComponents(
                        typeof(IReactOnDisposeEx<>), viewEngineDisposeEx, _reactiveEnginesDisposeEx, type.Name);

                if (engine is IReactOnSwap viewEngineSwap)
#pragma warning disable CS0612
#pragma warning disable CS0618
                    CheckReactEngineComponents(typeof(IReactOnSwap<>), viewEngineSwap, _reactiveEnginesSwap, type.Name);
#pragma warning restore CS0618
#pragma warning restore CS0612

                if (engine is IReactOnSwapEx viewEngineSwapEx)
                    CheckReactEngineComponents(
                        typeof(IReactOnSwapEx<>), viewEngineSwapEx, _reactiveEnginesSwapEx, type.Name);

                if (engine is IReactOnSubmission submissionEngine)
                    _reactiveEnginesSubmission.Add(submissionEngine);
                
                if (engine is IReactOnSubmissionStarted submissionEngineStarted)
                    _reactiveEnginesSubmissionStarted.Add(submissionEngineStarted);
                
                if (addSubEngines)
                if (engine is IGroupEngine stepGroupEngine)
                    foreach (var stepEngine in stepGroupEngine.engines)
                        AddEngine(stepEngine);
                
                _enginesTypeSet.Add(refWrapper);
                _enginesSet.Add(engine);

                if (engine is IDisposable)
                    _disposableEngines.Add(engine as IDisposable);

                if (engine is IQueryingEntitiesEngine queryableEntityComponentEngine)
                    queryableEntityComponentEngine.entitiesDB = _entitiesDB;

                //Ready is a callback that can be used to signal that the engine is ready to be used because the entitiesDB is now available
                if (_enginesWaitForReady == EnginesReadyOption.ReadyAsAdded && engine is IGetReadyEngine getReadyEngine)
                    getReadyEngine.Ready();
            }
            catch (Exception e)
            {
                throw new ECSException(
                    "Code crashed while adding engine ".FastConcat(engine.GetType().ToString(), " "),
                    e);
            }
        }

        public void Ready()
        {
            Check.Require(
                _enginesWaitForReady == EnginesReadyOption.WaitForReady,
                "The engine has not been initialise to wait for an external ready trigger");

            foreach (var engine in _enginesSet)
                if (engine is IGetReadyEngine getReadyEngine)
                    getReadyEngine.Ready();
        }

        static void AddEngineToList<T>(T engine, Type[] entityComponentTypes,
            FasterDictionary<ComponentID, FasterList<ReactEngineContainer<T>>> engines, string typeName)
            where T : class, IReactEngine
        {
            for (var i = 0; i < entityComponentTypes.Length; i++)
            {
                var type = entityComponentTypes[i];

                var componentID = ComponentTypeMap.FetchID(type);
                if (engines.TryGetValue(componentID, out var list) == false)
                {
                    list = new FasterList<ReactEngineContainer<T>>();

                    engines.Add(componentID, list);
                }

                list.Add(new ReactEngineContainer<T>(engine, typeName));
            }
        }

        void CheckReactEngineComponents<T>(Type genericDefinition, T engine,
            FasterDictionary<ComponentID, FasterList<ReactEngineContainer<T>>> engines, string typeName)
            where T : class, IReactEngine
        {
            var interfaces = engine.GetType().GetInterfaces();

            foreach (var interf in interfaces)
            {
                if (interf.IsGenericTypeEx() && interf.GetGenericTypeDefinition() == genericDefinition)
                {
                    Type[] genericArguments = interf.GetGenericArgumentsEx();

                    AddEngineToList(engine, genericArguments, engines, typeName);
                }
            }
        }

        void Dispose(bool disposing)
        {
            if (_isDisposed)
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
                        if (engine is IDisposableEngine dengine)
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

                            typeSafeDictionary.ExecuteEnginesDisposeCallbacks_Group(
                                _reactiveEnginesDispose, _reactiveEnginesDisposeEx, groups.key,
                                profiler);
                        }
                        catch (Exception e)
                        {
                            Console.LogException(e);
                        }

                foreach (var groups in _groupEntityComponentsDB)
                    foreach (var entityList in groups.value)
                        entityList.value.Dispose();

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
                _reactiveEnginesDisposeEx.Clear();
                _reactiveEnginesSubmission.Clear();
                _reactiveEnginesSubmissionStarted.Clear();

                _groupedEntityToAdd.Dispose();

                _entityLocator.DisposeEntityReferenceMap();

                _entityStreams.Dispose();
                scheduler.Dispose();
            }
            
            _isDisposed = true;
        }

        void NotifyReactiveEnginesOnSubmission()
        {
            var enginesCount = _reactiveEnginesSubmission.count;
            for (var i = 0; i < enginesCount; i++)
                _reactiveEnginesSubmission[i].EntitiesSubmitted();
        }
        
        void NotifyReactiveEnginesOnSubmissionStarted()
        {
            var enginesCount = _reactiveEnginesSubmissionStarted.count;
            for (var i = 0; i < enginesCount; i++)
                _reactiveEnginesSubmissionStarted[i].EntitiesSubmissionStarting();
        }

        public readonly struct EntitiesSubmitter
        {
            public EntitiesSubmitter(EnginesRoot enginesRoot): this()
            {
                _enginesRoot = new DataStructures.WeakReference<EnginesRoot>(enginesRoot);
            }

            internal void SubmitEntities()
            {
                Check.Require(_enginesRoot.IsValid, "ticking an GCed engines root?");

                var enginesRootTarget = _enginesRoot.Target;
                var entitiesSubmissionScheduler = enginesRootTarget.scheduler;

                if (entitiesSubmissionScheduler.paused == false)
                {
                    enginesRootTarget.NotifyReactiveEnginesOnSubmissionStarted();
                    Check.Require(
                        entitiesSubmissionScheduler.isRunning == false,
                        "A submission started while the previous one was still flushing");
                    entitiesSubmissionScheduler.isRunning = true;

                    using (var profiler = new PlatformProfiler("Svelto.ECS - Entities Submission"))
                    {
                        var iterations = 0;
                        var hasEverSubmitted = false;

                        // We need to clear transient filters before processing callbacks since the callbacks may add
                        // new entities to these filters.
                        enginesRootTarget.ClearTransientFilters();

#if UNITY_NATIVE
                        enginesRootTarget.FlushNativeOperations(profiler);
#endif
                        while (enginesRootTarget.HasMadeNewStructuralChangesInThisIteration()
                            && iterations++ < MAX_SUBMISSION_ITERATIONS)
                        {
                            hasEverSubmitted = true;

                            _enginesRoot.Target.SingleSubmission(profiler);
#if UNITY_NATIVE
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

            readonly DataStructures.WeakReference<EnginesRoot> _enginesRoot;
        }

        ~EnginesRoot()
        {
            Console.LogWarning("Engines Root has been garbage collected, don't forget to call Dispose()!");

            Dispose(false);
        }

        const int MAX_SUBMISSION_ITERATIONS = 10;

        readonly FasterList<IDisposable> _disposableEngines;
        readonly FasterList<IEngine> _enginesSet;
        readonly HashSet<Type> _enginesTypeSet;
        readonly EnginesReadyOption _enginesWaitForReady;

        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>> _reactiveEnginesAdd;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>> _reactiveEnginesAddEx;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> _reactiveEnginesRemove;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> _reactiveEnginesRemoveEx;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>> _reactiveEnginesSwap;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>> _reactiveEnginesSwapEx;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>> _reactiveEnginesDispose;
        readonly FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>> _reactiveEnginesDisposeEx;

        readonly FasterList<IReactOnSubmission> _reactiveEnginesSubmission;
        readonly FasterList<IReactOnSubmissionStarted> _reactiveEnginesSubmissionStarted;
        bool _isDisposed;
    }

    //Ready is a callback that can be used to signal that an engine is ready to be used because the entitiesDB is now available
    //usually engines are ready to be used when they are added to the enginesRoot, but in some special cases, it is possible to
    //wait for the user input to signal that the engine is ready to be used
    public enum EnginesReadyOption
    {
        ReadyAsAdded,
        WaitForReady
    }
}