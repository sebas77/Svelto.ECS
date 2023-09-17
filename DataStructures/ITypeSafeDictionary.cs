using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : _IInternalEntityComponent
    {
        uint Add(uint egidEntityId, in TValue entityComponent);
        
        bool       TryGetValue(uint entityId, out TValue item);
        ref TValue GetOrAdd(uint idEntityId);

        IBuffer<TValue> GetValues(out uint count);
        ref TValue      GetDirectValueByRef(uint key);
        ref TValue      GetValueByRef(uint key);
        IEntityIDs      entityIDs { get; }
    }

    public interface ITypeSafeDictionary : IDisposable
    {
        int                 count { get; }
        
        ITypeSafeDictionary Create();

        void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId
#if SLOW_SVELTO_SUBMISSION                             
       , in EnginesRoot.EntityReferenceMap entityLocator
#endif         
         );
        void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess, FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack);
        void SwapEntitiesBetweenDictionaries(in FasterDictionary<uint, SwapInfo> infosToProcess, ExclusiveGroupStruct fromGroup,
         ExclusiveGroupStruct toGroup
       , ITypeSafeDictionary toComponentsDictionary, FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack);
        
        //------------

        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesAddCallbacks
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDb
       , ITypeSafeDictionary destinationDatabase, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler);
        //Version to use
        void ExecuteEnginesAddEntityCallbacksFast(
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd,
         ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler);

        //------------
        
        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesSwapCallbacks(FasterDictionary<uint, SwapInfo> infosToProcess,
         FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
         ExclusiveGroupStruct toGroup, in PlatformProfiler sampler);
        //Version to use
        void ExecuteEnginesSwapCallbacksFast(FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap,
         ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies,
         in PlatformProfiler sampler);
        
        //------------
        
        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesRemoveCallbacks(FasterList<(uint, string)> infosToProcess,
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
         ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler);
        //Version to use
        void ExecuteEnginesRemoveCallbacksFast(FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx,
         ExclusiveGroupStruct fromGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies,
         in PlatformProfiler sampler);
        
        //------------

        void ExecuteEnginesSwapCallbacks_Group(
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap,
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx,
         ITypeSafeDictionary toEntitiesDictionary, ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId,
         in PlatformProfiler platformProfiler);
        void ExecuteEnginesRemoveCallbacks_Group(
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx,
         ExclusiveGroupStruct @group, in PlatformProfiler profiler);
        void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>> reactiveEnginesDispose,
         FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>> reactiveEnginesDisposeEx,
        ExclusiveGroupStruct group, in PlatformProfiler profiler);

        void IncreaseCapacityBy(uint size);
        void EnsureCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint key);
        bool ContainsKey(uint egidEntityId);
        uint GetIndex(uint valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);

        void KeysEvaluator(Action<uint> action);
    }
}