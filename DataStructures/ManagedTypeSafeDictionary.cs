using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    sealed class ManagedTypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue>
        where TValue : struct, _IInternalEntityComponent
    {
        //todo: would this be better to not be static to avoid overhead?
        static readonly ThreadLocal<IEntityIDs> cachedEntityIDM =
            new ThreadLocal<IEntityIDs>(() => new ManagedEntityIDs());

        public ManagedTypeSafeDictionary(uint size)
        {
            implMgd = new SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
                    ManagedStrategy<int>>(size, Allocator.Managed);
        }
        
        public IEntityIDs entityIDs
        {
            get
            {
                ref var unboxed = ref Unsafe.Unbox<ManagedEntityIDs>(cachedEntityIDM.Value);

                unboxed.Update(implMgd.unsafeKeys.ToRealBuffer());

                return cachedEntityIDM.Value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return implMgd.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return implMgd.GetIndex(valueEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrAdd(uint idEntityId)
        {
            return ref implMgd.GetOrAdd(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IBuffer<TValue> GetValues(out uint count)
        {
            return implMgd.UnsafeGetValues(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValueByRef(uint key)
        {
            return ref implMgd.GetDirectValueByRef(key);
        }

        public ref TValue GetValueByRef(uint key)
        {
            return ref implMgd.GetValueByRef(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key)
        {
            return implMgd.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index)
        {
            return implMgd.TryFindIndex(entityId, out index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item)
        {
            return implMgd.TryGetValue(entityId, out item);
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => implMgd.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return new ManagedTypeSafeDictionary<TValue>(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            implMgd.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureCapacity(uint size)
        {
            implMgd.EnsureCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncreaseCapacityBy(uint size)
        {
            implMgd.IncreaseCapacityBy(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            implMgd.Trim();
        }

        public void KeysEvaluator(Action<uint> action)
        {
            foreach (var key in implMgd.keys)
                action(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(uint egidEntityId, in TValue entityComponent)
        {
            if (implMgd.TryAdd(egidEntityId, entityComponent, out uint index) == false)
                throw new TypeSafeDictionaryException("Key already present");

            return index;
        }

        public void Dispose()
        {
            implMgd.Dispose();

            GC.SuppressFinalize(this);
        }

        /// *********************************
        /// the following methods are executed during the submission of entities
        /// *********************************
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId
#if SLOW_SVELTO_SUBMISSION
               , in EnginesRoot.EntityReferenceMap entityLocator
#endif
        )
        {
            TypeSafeDictionaryMethods.AddEntitiesToDictionary(implMgd, toDictionary as ITypeSafeDictionary<TValue>
#if SLOW_SVELTO_SUBMISSION
, entityLocator
#endif
                                                            , groupId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess, FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack)
        {
            TypeSafeDictionaryMethods.RemoveEntitiesFromDictionary(infosToProcess, ref implMgd, entityIDsAffectedByRemoveAtSwapBack);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SwapEntitiesBetweenDictionaries(in FasterDictionary<uint, SwapInfo> infosToProcess,
            ExclusiveGroupStruct fromGroup
          , ExclusiveGroupStruct toGroup, ITypeSafeDictionary toComponentsDictionary
          , FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack)
        {
            TypeSafeDictionaryMethods.SwapEntitiesBetweenDictionaries(infosToProcess, ref implMgd
                                                                    , toComponentsDictionary as ITypeSafeDictionary<TValue>, fromGroup
                                                                    , toGroup, entityIDsAffectedByRemoveAtSwapBack);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAdd callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddCallbacks
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDB
       , ITypeSafeDictionary toDic, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddCallbacks(ref implMgd, (ITypeSafeDictionary<TValue>)toDic
                                                               , toGroup, entityComponentEnginesDB, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacks(FasterDictionary<uint, SwapInfo> infosToProcess
          , FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
          , ExclusiveGroupStruct toGroup, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks(infosToProcess, ref implMgd, reactiveEnginesSwap
                                                                , toGroup, fromGroup, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnREmove callbacks linked to components removed this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks
        (FasterList<(uint, string)> infosToProcess
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks(infosToProcess, ref implMgd, reactiveEnginesRemove
                                                                  , fromGroup, in sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnAddEx callbacks linked to components added this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesAddEntityCallbacksFast
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd
       , ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesAddEntityCallbacksFast(
                reactiveEnginesAdd, groupID, rangeOfSubmittedEntitiesIndicies, entityIDs, this, profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwapEx callbacks linked to components swapped this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacksFast(reactiveEnginesSwap, fromGroup, toGroup, entityIDs
                                                                    , this, rangeOfSubmittedEntitiesIndicies, sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemoveEx callbacks linked to components removed this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacksFast
        (FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx, ExclusiveGroupStruct fromGroup
       , (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler sampler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacksFast(reactiveEnginesRemoveEx, fromGroup
                                                                      , rangeOfSubmittedEntitiesIndicies, entityIDs
                                                                      , this, sampler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnSwap and IReactOnSwapEx callbacks linked to components swapped between
        ///     whole groups swapped during this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesSwapCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx
       , ITypeSafeDictionary toDictionary, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup
       , in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesSwapCallbacks_Group(
                ref implMgd, (ITypeSafeDictionary<TValue>)toDictionary, toGroup, fromGroup, reactiveEnginesSwap
              , reactiveEnginesSwapEx, entityIDs, in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnRemove and IReactOnRemoveEx callbacks linked to components remove from
        ///     whole groups removed during this submit
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesRemoveCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove
       , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx
       , ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesRemoveCallbacks_Group(
                ref implMgd, reactiveEnginesRemove, reactiveEnginesRemoveEx, entityIDs, group
              , in profiler);
        }

        /// <summary>
        ///     Execute all the engine IReactOnDispose for each component registered in the DB when it's disposed of
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>> reactiveEnginesDispose,
        FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>> reactiveEnginesDisposeEx,
        ExclusiveGroupStruct group, in PlatformProfiler profiler)
        {
            TypeSafeDictionaryMethods.ExecuteEnginesDisposeCallbacks_Group(
                ref implMgd, reactiveEnginesDispose, reactiveEnginesDisposeEx, entityIDs, group, in profiler);
        }

        internal SveltoDictionary<uint, TValue, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<TValue>,
            ManagedStrategy<int>> implMgd;
    }
}